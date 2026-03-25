<?php
/**
 * @package     ShowcaseResults
 * @subpackage  com_showcaseresults
 * @copyright   Copyright (C) 2026 mlinnen. All rights reserved.
 * @license     GNU General Public License version 2 or later
 */

namespace Mlinnen\Component\ShowcaseResults\Site\Service;

defined('_JEXEC') or die;

use Joomla\CMS\Log\Log;

/**
 * Service for loading and querying showcase results data
 *
 * Provides three lookup modes:
 * 1. Name cross-event: Searches all results-*.json files for a name
 * 2. Name single-event: Searches one results file for a name
 * 3. ID single-event: Searches one results file for a carver_id
 *
 * CRITICAL: carver_id is per-event only (privacy by design).
 * Do NOT attempt to correlate carver_id across events.
 *
 * @since 1.0.0
 */
class ResultsService
{
    /**
     * Path to the data directory containing results-*.json files
     *
     * @var string
     */
    private $dataPath;

    /**
     * Constructor
     */
    public function __construct()
    {
        $this->dataPath = JPATH_ROOT . '/media/com_showcaseresults/data';
    }

    /**
     * Main entry point — dispatches to the correct lookup mode
     *
     * @param   string  $name       Carver name (first + last)
     * @param   int     $carver_id  Per-event carver identifier
     * @param   int     $year       Event year
     *
     * @return  array   Lookup results
     */
    public function lookup(string $name = '', int $carver_id = 0, int $year = 0): array
    {
        // Validate: carver_id requires year
        if ($carver_id > 0 && $year === 0)
        {
            return [
                'error' => 'carver_id_requires_year',
                'results' => []
            ];
        }

        // Mode 1: Name cross-event
        if (!empty($name) && $year === 0)
        {
            return $this->lookupByName($name);
        }

        // Mode 2: Name single-event
        if (!empty($name) && $year > 0)
        {
            return $this->lookupByNameAndYear($name, $year);
        }

        // Mode 3: ID single-event
        if ($carver_id > 0 && $year > 0)
        {
            return $this->lookupByCarverIdAndYear($carver_id, $year);
        }

        // No valid lookup parameters
        return [
            'error' => 'no_lookup_parameters',
            'results' => []
        ];
    }

    /**
     * Scan all results files for a name (cross-event)
     *
     * @param   string  $name  Carver name (first + last)
     *
     * @return  array   Results grouped by event, most recent first
     */
    private function lookupByName(string $name): array
    {
        $files = $this->getResultsFiles();

        if (empty($files))
        {
            return [
                'error' => 'no_data',
                'results' => []
            ];
        }

        $allResults = [];
        $carverName = '';

        foreach ($files as $filepath)
        {
            $data = $this->loadResultsFile($filepath);

            if ($data === null)
            {
                continue;
            }

            // Search competitors for name match
            $carverId = $this->findCarverIdByName($data, $name);

            if ($carverId === null)
            {
                continue;
            }

            // Extract results for this carver in this event
            $eventResults = $this->extractCarverResults($data, $carverId);

            if (!empty($eventResults))
            {
                $allResults[] = [
                    'event_name' => $data['event']['name'] ?? '',
                    'event_year' => $data['event']['year'] ?? 0,
                    'special_prizes' => $eventResults['special_prizes'],
                    'overall_results' => $eventResults['overall_results'],
                    'division_results' => $eventResults['division_results']
                ];

                // Capture the name from competitors (first match wins)
                if (empty($carverName) && isset($data['competitors']))
                {
                    foreach ($data['competitors'] as $comp)
                    {
                        if ($comp['carver_id'] === $carverId)
                        {
                            $carverName = trim($comp['first_name'] . ' ' . $comp['last_name']);
                            break;
                        }
                    }
                }
            }
        }

        // Sort by year descending
        usort($allResults, function($a, $b) {
            return $b['event_year'] - $a['event_year'];
        });

        return [
            'carver_name' => $carverName,
            'found' => !empty($allResults),
            'results' => $allResults
        ];
    }

    /**
     * Scan one results file for a name (single-event)
     *
     * @param   string  $name  Carver name (first + last)
     * @param   int     $year  Event year
     *
     * @return  array   Results for the specified event
     */
    private function lookupByNameAndYear(string $name, int $year): array
    {
        $filepath = $this->dataPath . '/results-' . $year . '.json';

        if (!file_exists($filepath))
        {
            return [
                'found' => false,
                'results' => []
            ];
        }

        $data = $this->loadResultsFile($filepath);

        if ($data === null)
        {
            return [
                'error' => 'data_load_error',
                'found' => false,
                'results' => []
            ];
        }

        // Search competitors for name match
        $carverId = $this->findCarverIdByName($data, $name);

        if ($carverId === null)
        {
            return [
                'found' => false,
                'results' => []
            ];
        }

        // Extract results for this carver
        $eventResults = $this->extractCarverResults($data, $carverId);

        $carverName = '';
        if (isset($data['competitors']))
        {
            foreach ($data['competitors'] as $comp)
            {
                if ($comp['carver_id'] === $carverId)
                {
                    $carverName = trim($comp['first_name'] . ' ' . $comp['last_name']);
                    break;
                }
            }
        }

        return [
            'carver_name' => $carverName,
            'found' => !empty($eventResults['special_prizes']) || 
                       !empty($eventResults['overall_results']) || 
                       !empty($eventResults['division_results']),
            'results' => [
                [
                    'event_name' => $data['event']['name'] ?? '',
                    'event_year' => $data['event']['year'] ?? 0,
                    'special_prizes' => $eventResults['special_prizes'],
                    'overall_results' => $eventResults['overall_results'],
                    'division_results' => $eventResults['division_results']
                ]
            ]
        ];
    }

    /**
     * Scan one results file for a carver_id (single-event only)
     *
     * @param   int  $carver_id  Per-event carver identifier
     * @param   int  $year       Event year
     *
     * @return  array   Results for the specified carver
     */
    private function lookupByCarverIdAndYear(int $carver_id, int $year): array
    {
        $filepath = $this->dataPath . '/results-' . $year . '.json';

        if (!file_exists($filepath))
        {
            return [
                'found' => false,
                'results' => []
            ];
        }

        $data = $this->loadResultsFile($filepath);

        if ($data === null)
        {
            return [
                'error' => 'data_load_error',
                'found' => false,
                'results' => []
            ];
        }

        // Extract results for this carver
        $eventResults = $this->extractCarverResults($data, $carver_id);

        $carverName = '';
        if (isset($data['competitors']))
        {
            foreach ($data['competitors'] as $comp)
            {
                if ($comp['carver_id'] === $carver_id)
                {
                    $carverName = trim($comp['first_name'] . ' ' . $comp['last_name']);
                    break;
                }
            }
        }

        return [
            'carver_name' => $carverName,
            'found' => !empty($eventResults['special_prizes']) || 
                       !empty($eventResults['overall_results']) || 
                       !empty($eventResults['division_results']),
            'results' => [
                [
                    'event_name' => $data['event']['name'] ?? '',
                    'event_year' => $data['event']['year'] ?? 0,
                    'special_prizes' => $eventResults['special_prizes'],
                    'overall_results' => $eventResults['overall_results'],
                    'division_results' => $eventResults['division_results']
                ]
            ]
        ];
    }

    /**
     * Helper: load and parse a single JSON file
     *
     * @param   string  $filepath  Full path to JSON file
     *
     * @return  array|null  Parsed data or null on error
     */
    private function loadResultsFile(string $filepath): ?array
    {
        if (!file_exists($filepath))
        {
            return null;
        }

        $content = file_get_contents($filepath);

        if ($content === false)
        {
            Log::add('Failed to read file: ' . $filepath, Log::WARNING, 'com_showcaseresults');
            return null;
        }

        $data = json_decode($content, true);

        if ($data === null)
        {
            Log::add('Failed to parse JSON in file: ' . $filepath, Log::WARNING, 'com_showcaseresults');
            return null;
        }

        return $data;
    }

    /**
     * Helper: get all results-*.json file paths
     *
     * @return  array  Array of file paths, sorted by year descending
     */
    private function getResultsFiles(): array
    {
        if (!is_dir($this->dataPath))
        {
            return [];
        }

        $files = glob($this->dataPath . '/results-*.json');

        if ($files === false)
        {
            return [];
        }

        // Sort by filename (year) descending
        rsort($files);

        return $files;
    }

    /**
     * Helper: filter event data for a specific carver_id
     *
     * @param   array  $data       Parsed results data
     * @param   int    $carver_id  Carver identifier
     *
     * @return  array  Filtered results arrays
     */
    private function extractCarverResults(array $data, int $carver_id): array
    {
        $results = [
            'special_prizes' => [],
            'overall_results' => [],
            'division_results' => []
        ];

        // Filter special prizes
        if (isset($data['special_prizes']))
        {
            foreach ($data['special_prizes'] as $prize)
            {
                if (isset($prize['carver_id']) && $prize['carver_id'] === $carver_id)
                {
                    $results['special_prizes'][] = $prize;
                }
            }
        }

        // Filter overall results
        if (isset($data['overall_results']))
        {
            foreach ($data['overall_results'] as $category)
            {
                $matchingPlaces = [];

                if (isset($category['places']))
                {
                    foreach ($category['places'] as $place)
                    {
                        if (isset($place['carver_id']) && $place['carver_id'] === $carver_id)
                        {
                            $matchingPlaces[] = $place;
                        }
                    }
                }

                if (!empty($matchingPlaces))
                {
                    $results['overall_results'][] = [
                        'category' => $category['category'] ?? '',
                        'places' => $matchingPlaces
                    ];
                }
            }
        }

        // Filter division results
        if (isset($data['division_results']))
        {
            foreach ($data['division_results'] as $division)
            {
                $matchingCategories = [];

                if (isset($division['categories']))
                {
                    foreach ($division['categories'] as $category)
                    {
                        $matchingPlaces = [];

                        if (isset($category['places']))
                        {
                            foreach ($category['places'] as $place)
                            {
                                if (isset($place['carver_id']) && $place['carver_id'] === $carver_id)
                                {
                                    $matchingPlaces[] = $place;
                                }
                            }
                        }

                        if (!empty($matchingPlaces))
                        {
                            $matchingCategories[] = [
                                'name' => $category['name'] ?? '',
                                'style' => $category['style'] ?? null,
                                'places' => $matchingPlaces
                            ];
                        }
                    }
                }

                if (!empty($matchingCategories))
                {
                    $results['division_results'][] = [
                        'division' => $division['division'] ?? '',
                        'categories' => $matchingCategories
                    ];
                }
            }
        }

        return $results;
    }

    /**
     * Helper: find carver_id by name match in competitors array
     *
     * @param   array   $data  Parsed results data
     * @param   string  $name  Name to search for (case-insensitive)
     *
     * @return  int|null  Carver ID or null if not found
     */
    private function findCarverIdByName(array $data, string $name): ?int
    {
        if (!isset($data['competitors']) || !is_array($data['competitors']))
        {
            return null;
        }

        $searchName = strtolower(trim($name));

        foreach ($data['competitors'] as $comp)
        {
            if (!isset($comp['first_name']) || !isset($comp['last_name']) || !isset($comp['carver_id']))
            {
                continue;
            }

            $fullName = strtolower(trim($comp['first_name'] . ' ' . $comp['last_name']));

            if ($fullName === $searchName)
            {
                return $comp['carver_id'];
            }
        }

        return null;
    }
}

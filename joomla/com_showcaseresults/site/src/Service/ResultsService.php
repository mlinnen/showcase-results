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
     * @param   string  $event      Event identifier
     *
     * @return  array   Lookup results
     */
    public function lookup(string $name = '', int $carver_id = 0, string $event = ''): array
    {
        // Mode 1: Name cross-event
        if (!empty($name) && $event === '')
        {
            return $this->lookupByName($name);
        }

        // Mode 2: Name single-event
        if (!empty($name) && !empty($event))
        {
            return $this->lookupByNameAndEvent($name, $event);
        }

        // Mode 3: ID single-event
        if ($carver_id > 0 && !empty($event))
        {
            return $this->lookupByCarverIdAndEvent($carver_id, $event);
        }

        // Should not reach here due to validation in HtmlView
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
                'error' => 'no_data_files',
                'error_message' => 'No competition data is currently available. Please check back later.',
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
                    'event_year' => $data['event']['event_id'] ?? 0,
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

        // Check if name was not found in any event
        if (empty($allResults))
        {
            return [
                'error' => 'name_not_found',
                'error_message' => "No results found for '" . $name . "'. Please check the spelling and try again.",
                'search_name' => $name,
                'results' => []
            ];
        }

        return [
            'carver_name' => $carverName,
            'found' => true,
            'results' => $allResults
        ];
    }

    /**
     * Scan one results file for a name (single-event)
     *
     * @param   string  $name  Carver name (first + last)
     * @param   string  $event  Event identifier
     *
     * @return  array   Results for the specified event
     */
    private function lookupByNameAndEvent(string $name, string $event): array
    {
        $filepath = $this->dataPath . '/results-' . $event . '.json';

        if (!file_exists($filepath))
        {
            // Get available events
            $availableEvents = $this->getAvailableEvents();
            $eventsList = empty($availableEvents) ? 'none' : implode(', ', $availableEvents);

            return [
                'error' => 'event_not_found',
                'error_message' => "No data available for {$event}. Available events: {$eventsList}.",
                'search_event' => $event,
                'results' => []
            ];
        }

        $data = $this->loadResultsFile($filepath);

        if ($data === null)
        {
            return [
                'error' => 'data_load_error',
                'error_message' => "Data for {$event} is temporarily unavailable.",
                'search_event' => $event,
                'results' => []
            ];
        }

        // Search competitors for name match
        $carverId = $this->findCarverIdByName($data, $name);

        if ($carverId === null)
        {
            return [
                'error' => 'name_not_found_in_event',
                'error_message' => "No results found for '{$name}' in {$event}. They may have competed in a different event.",
                'search_name' => $name,
                'search_event' => $event,
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

        // Check if carver is registered but has no results
        $hasResults = !empty($eventResults['special_prizes']) || 
                      !empty($eventResults['overall_results']) || 
                      !empty($eventResults['division_results']);

        if (!$hasResults)
        {
            return [
                'error' => 'no_results_for_carver',
                'error_message' => "{$carverName} is a checked-in competitor in {$event} but has no recorded results.",
                'carver_name' => $carverName,
                'search_event' => $event,
                'results' => []
            ];
        }

        return [
            'carver_name' => $carverName,
            'found' => true,
            'results' => [
                [
                    'event_name' => $data['event']['name'] ?? '',
                    'event_year' => $data['event']['event_id'] ?? 0,
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
     * @param   int     $carver_id  Per-event carver identifier
     * @param   string  $event      Event identifier
     *
     * @return  array   Results for the specified carver
     */
    private function lookupByCarverIdAndEvent(int $carver_id, string $event): array
    {
        $filepath = $this->dataPath . '/results-' . $event . '.json';

        if (!file_exists($filepath))
        {
            // Get available events
            $availableEvents = $this->getAvailableEvents();
            $eventsList = empty($availableEvents) ? 'none' : implode(', ', $availableEvents);

            return [
                'error' => 'event_not_found',
                'error_message' => "No data available for {$event}. Available events: {$eventsList}.",
                'search_event' => $event,
                'results' => []
            ];
        }

        $data = $this->loadResultsFile($filepath);

        if ($data === null)
        {
            return [
                'error' => 'data_load_error',
                'error_message' => "Data for {$event} is temporarily unavailable.",
                'search_event' => $event,
                'results' => []
            ];
        }

        // Check if carver_id exists in competitors
        $carverName = '';
        $carverExists = false;
        if (isset($data['competitors']))
        {
            foreach ($data['competitors'] as $comp)
            {
                if ($comp['carver_id'] === $carver_id)
                {
                    $carverName = trim($comp['first_name'] . ' ' . $comp['last_name']);
                    $carverExists = true;
                    break;
                }
            }
        }

        if (!$carverExists)
        {
            return [
                'error' => 'carver_id_not_found',
                'error_message' => "Carver #{$carver_id} was not found in the {$event} results.",
                'search_carver_id' => $carver_id,
                'search_event' => $event,
                'results' => []
            ];
        }

        // Extract results for this carver
        $eventResults = $this->extractCarverResults($data, $carver_id);

        // Check if carver is registered but has no results
        $hasResults = !empty($eventResults['special_prizes']) || 
                      !empty($eventResults['overall_results']) || 
                      !empty($eventResults['division_results']);

        if (!$hasResults)
        {
            return [
                'error' => 'no_results_for_carver',
                'error_message' => "{$carverName} is a checked-in competitor in {$event} but has no recorded results.",
                'carver_name' => $carverName,
                'search_event' => $event,
                'results' => []
            ];
        }

        return [
            'carver_name' => $carverName,
            'found' => true,
            'results' => [
                [
                    'event_name' => $data['event']['name'] ?? '',
                    'event_year' => $data['event']['event_id'] ?? 0,
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
     * Get list of all competitors for a given event
     *
     * Returns an array with keys: event_name, event_year, carvers (sorted by carver_id ascending).
     * Each carver entry: carver_id, first_name, last_name, full_name, division.
     * The JSON competitors array includes all checked-in competitors plus any result-bearing
     * competitors needed for lookup integrity. This list view should show only checked-in rows.
     *
     * @param   string  $event  Event identifier
     *
     * @return  array  Carvers list data, or error array if event/data unavailable
     */
    public function getCarversList(string $event): array
    {
        $filepath = $this->dataPath . '/results-' . $event . '.json';

        if (!file_exists($filepath))
        {
            $availableEvents = $this->getAvailableEvents();
            $eventsList = empty($availableEvents) ? 'none' : implode(', ', $availableEvents);

            return [
                'error'         => 'event_not_found',
                'error_message' => "No data available for {$event}. Available events: {$eventsList}.",
                'search_event'  => $event,
                'carvers'       => [],
            ];
        }

        $data = $this->loadResultsFile($filepath);

        if ($data === null)
        {
            return [
                'error'         => 'data_load_error',
                'error_message' => "Data for {$event} is temporarily unavailable.",
                'search_event'  => $event,
                'carvers'       => [],
            ];
        }

        $carvers = [];

        foreach ($data['competitors'] ?? [] as $comp)
        {
            if (array_key_exists('checked_in', $comp) && !$comp['checked_in'])
            {
                continue;
            }

            $carverId = $comp['carver_id'] ?? 0;
            $firstName = $comp['first_name'] ?? '';
            $lastName  = $comp['last_name'] ?? '';

            $carvers[] = [
                'carver_id'  => $carverId,
                'first_name' => $firstName,
                'last_name'  => $lastName,
                'full_name'  => trim($firstName . ' ' . $lastName),
                'division'   => $comp['division'] ?? '',
            ];
        }

        // Sort by carver_id ascending
        usort($carvers, function (array $a, array $b): int {
            return $a['carver_id'] <=> $b['carver_id'];
        });

        return [
            'event_name'  => $data['event']['name'] ?? '',
            'event_year'  => $data['event']['event_id'] ?? $event,
            'carvers'     => $carvers,
        ];
    }

    /**
     * Get list of available events from results files
     *
     * @return  array  Array of event identifiers (strings), sorted descending
     */
    public function getAvailableEvents(): array
    {
        $files = $this->getResultsFiles();
        $events = [];

        foreach ($files as $filepath)
        {
            // Extract event identifier from filename: results-2024.json or results-2026T.json -> 2024 or 2026T
            if (preg_match('/results-([a-zA-Z0-9]+)\.json$/', basename($filepath), $matches))
            {
                $events[] = $matches[1];
            }
        }

        rsort($events);
        return $events;
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

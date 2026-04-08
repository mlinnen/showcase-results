<?php
/**
 * @package     ShowcaseResults
 * @subpackage  com_showcaseresults
 * @copyright   Copyright (C) 2026 mlinnen. All rights reserved.
 * @license     GNU General Public License version 2 or later
 */

namespace Mlinnen\Component\ShowcaseResults\Site\View\Carver;

defined('_JEXEC') or die;

use Joomla\CMS\MVC\View\HtmlView as BaseHtmlView;
use Joomla\CMS\Factory;
use Mlinnen\Component\ShowcaseResults\Site\Service\ResultsService;

/**
 * HTML view for Carver results
 *
 * Accepts query parameters:
 * - name: Carver name for cross-event lookup
 * - carver_id: Per-event carver identifier (requires event parameter)
 * - event: Event identifier (required when using carver_id)
 *
 * Note: carver_id without event is invalid (privacy by design).
 * Template rendering will be enhanced in issue #11.
 * Error handling for invalid params will be added in issue #12.
 *
 * @since 1.0.0
 */
class HtmlView extends BaseHtmlView
{
    /**
     * Carver data from ResultsService
     *
     * @var array
     */
    public $carverData;

    /**
     * Display the view
     *
     * @param   string  $tpl  The name of the template file to parse
     *
     * @return  void
     */
    public function display($tpl = null)
    {
        $app = Factory::getApplication();
        $input = $app->input;

        // Get raw query parameters
        $nameRaw = $input->getString('name', '');
        $carverIdRaw = $input->getString('carver_id', '');
        $eventRaw = $input->getString('event', '');

        // Validate and sanitize inputs
        $validation = $this->validateParameters($nameRaw, $carverIdRaw, $eventRaw);

        if ($validation['error'])
        {
            $this->carverData = [
                'error' => $validation['error'],
                'error_message' => $validation['message'],
                'results' => []
            ];
        }
        else
        {
            // Load data via ResultsService
            $service = new ResultsService();
            $this->carverData = $service->lookup(
                $validation['name'],
                $validation['carver_id'],
                $validation['event']
            );
        }

        // Set page title
        if (!empty($this->carverData['carver_name']))
        {
            $this->document->setTitle($this->carverData['carver_name'] . ' - Showcase Results');
        }
        else
        {
            $this->document->setTitle('Carver Results');
        }

        parent::display($tpl);
    }

    /**
     * Validate and sanitize input parameters
     *
     * @param   string  $nameRaw       Raw name parameter
     * @param   string  $carverIdRaw   Raw carver_id parameter
     * @param   string  $eventRaw      Raw event parameter
     *
     * @return  array   Validation result with error or sanitized values
     */
    private function validateParameters(string $nameRaw, string $carverIdRaw, string $eventRaw): array
    {
        $name = trim($nameRaw);
        $carver_id = 0;
        $event = '';

        // Check if no parameters provided
        if (empty($name) && empty($carverIdRaw) && empty($eventRaw))
        {
            return [
                'error' => 'no_parameters',
                'message' => 'No search parameters provided.',
                'name' => '',
                'carver_id' => 0,
                'event' => ''
            ];
        }

        // Validate carver_id if provided
        if (!empty($carverIdRaw))
        {
            if (!is_numeric($carverIdRaw))
            {
                return [
                    'error' => 'invalid_carver_id',
                    'message' => 'Carver ID must be a number.',
                    'name' => '',
                    'carver_id' => 0,
                    'event' => ''
                ];
            }
            $carver_id = (int) $carverIdRaw;
        }

        // Validate event if provided
        if (!empty($eventRaw))
        {
            if (!preg_match('/^[a-zA-Z0-9]+$/', $eventRaw))
            {
                return [
                    'error' => 'invalid_event',
                    'message' => 'Event must contain only letters and numbers.',
                    'name' => '',
                    'carver_id' => 0,
                    'event' => ''
                ];
            }
            $event = $eventRaw;
        }

        // Validate carver_id requires event
        if ($carver_id > 0 && $event === '')
        {
            return [
                'error' => 'carver_id_requires_event',
                'message' => 'An event is required when looking up by carver ID, because carver IDs differ between events. Try adding &event=2024 or search by name instead.',
                'name' => '',
                'carver_id' => 0,
                'event' => ''
            ];
        }

        // Name takes precedence over carver_id (no error, just ignore carver_id)
        if (!empty($name) && $carver_id > 0)
        {
            $carver_id = 0;
        }

        return [
            'error' => false,
            'message' => '',
            'name' => $name,
            'carver_id' => $carver_id,
            'event' => $event
        ];
    }
}

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
 * - carver_id: Per-event carver identifier (requires year parameter)
 * - year: Event year (required when using carver_id)
 *
 * Note: carver_id without year is invalid (privacy by design).
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

        // Get query parameters
        $name = $input->getString('name', '');
        $carver_id = $input->getInt('carver_id', 0);
        $year = $input->getInt('year', 0);

        // Load data via ResultsService
        $service = new ResultsService();
        $this->carverData = $service->lookup($name, $carver_id, $year);

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
}

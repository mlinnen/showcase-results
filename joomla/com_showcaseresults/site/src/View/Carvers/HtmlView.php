<?php
/**
 * @package     ShowcaseResults
 * @subpackage  com_showcaseresults
 * @copyright   Copyright (C) 2026 mlinnen. All rights reserved.
 * @license     GNU General Public License version 2 or later
 */

namespace Mlinnen\Component\ShowcaseResults\Site\View\Carvers;

defined('_JEXEC') or die;

use Joomla\CMS\MVC\View\HtmlView as BaseHtmlView;
use Joomla\CMS\Factory;
use Mlinnen\Component\ShowcaseResults\Site\Service\ResultsService;

/**
 * HTML view for the Carvers list
 *
 * Accepts query parameters:
 * - year: Event year to filter by (shows year selector if omitted)
 *
 * @since 1.0.0
 */
class HtmlView extends BaseHtmlView
{
    /**
     * Carvers list data from ResultsService
     *
     * @var array
     */
    public $carversData;

    /**
     * Display the view
     *
     * @param   string  $tpl  The name of the template file to parse
     *
     * @return  void
     */
    public function display($tpl = null)
    {
        $app   = Factory::getApplication();
        $input = $app->input;
        $year  = $input->getString('year', '');

        $service = new ResultsService();

        if ($year === '')
        {
            // No year supplied — pass available years so template can render selector
            $this->carversData = [
                'no_year'         => true,
                'available_years' => $service->getAvailableYears(),
            ];
            $this->document->setTitle('Carvers List');
        }
        else
        {
            $this->carversData = $service->getCarversList($year);

            if (!empty($this->carversData['event_name']))
            {
                $this->document->setTitle(
                    'Carvers - ' . $this->carversData['event_name'] . ' ' . $this->carversData['event_year']
                );
            }
            else
            {
                $this->document->setTitle('Carvers List');
            }
        }

        parent::display($tpl);
    }
}

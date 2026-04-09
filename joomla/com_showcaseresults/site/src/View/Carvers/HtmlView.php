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
 * - event: Event to filter by (shows event selector if omitted)
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
        $event  = $input->getString('event', '');

        $service = new ResultsService();

        if ($event === '')
        {
            // No event supplied — pass available events so template can render selector
            $this->carversData = [
                'no_event'         => true,
                'available_events' => $service->getAvailableEvents(),
            ];
            $this->document->setTitle('Carvers List');
        }
        else
        {
            $this->carversData = $service->getCarversList($event);

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

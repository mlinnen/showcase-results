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

/**
 * HTML view for Carver results
 *
 * Accepts query parameters:
 * - name: Carver name for cross-event lookup
 * - carver_id: Per-event carver identifier (requires year parameter)
 * - year: Event year (required when using carver_id)
 *
 * Note: carver_id without year is invalid (privacy by design).
 * Data loading logic will be implemented in issue #10.
 * Template rendering will be enhanced in issue #11.
 * Error handling for invalid params will be added in issue #12.
 *
 * @since 1.0.0
 */
class HtmlView extends BaseHtmlView
{
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
        $this->name = $input->getString('name', '');
        $this->carver_id = $input->getInt('carver_id', 0);
        $this->year = $input->getInt('year', 0);

        // Business logic placeholder — data loading comes in issue #10
        // Error handling for carver_id without year comes in issue #12

        parent::display($tpl);
    }
}

<?php
/**
 * @package     ShowcaseResults
 * @subpackage  com_showcaseresults
 * @copyright   Copyright (C) 2026 mlinnen. All rights reserved.
 * @license     GNU General Public License version 2 or later
 */

namespace Mlinnen\Component\ShowcaseResults\Site\Controller;

defined('_JEXEC') or die;

use Joomla\CMS\MVC\Controller\BaseController;

/**
 * Default display controller for Showcase Results component
 *
 * Joomla 4+ routes requests with no explicit task through DisplayController.
 *
 * @since 1.0.0
 */
class DisplayController extends BaseController
{
    /**
     * The default view
     *
     * @var string
     */
    protected $default_view = 'carver';
}

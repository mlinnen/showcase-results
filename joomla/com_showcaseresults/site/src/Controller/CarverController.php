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
 * Carver controller for Showcase Results component
 *
 * Handles display of per-carver competition results.
 * Business logic for data loading will be implemented in issue #10.
 *
 * @since 1.0.0
 */
class CarverController extends BaseController
{
    /**
     * The default view
     *
     * @var string
     */
    protected $default_view = 'carver';
}

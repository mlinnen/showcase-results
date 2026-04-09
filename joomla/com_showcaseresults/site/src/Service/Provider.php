<?php
/**
 * @package     ShowcaseResults
 * @subpackage  com_showcaseresults
 * @copyright   Copyright (C) 2026 mlinnen. All rights reserved.
 * @license     GNU General Public License version 2 or later
 */

namespace Mlinnen\Component\ShowcaseResults\Site\Service;

defined('_JEXEC') or die;

use Joomla\CMS\Extension\Service\Provider\CategoryFactory;
use Joomla\CMS\Extension\Service\Provider\ComponentDispatcherFactory;
use Joomla\CMS\Extension\Service\Provider\MVCFactory;
use Joomla\CMS\MVC\Factory\MVCFactoryInterface;
use Joomla\DI\Container;
use Joomla\DI\ServiceProviderInterface;

/**
 * Service provider for Showcase Results component
 *
 * Registers the component with Joomla's dependency injection container.
 *
 * @since 1.0.0
 */
class Provider implements ServiceProviderInterface
{
    /**
     * Registers the service provider with a DI container.
     *
     * @param   Container  $container  The DI container.
     *
     * @return  void
     */
    public function register(Container $container)
    {
        $container->registerServiceProvider(new CategoryFactory('\\Mlinnen\\Component\\ShowcaseResults'));
        $container->registerServiceProvider(new MVCFactory('\\Mlinnen\\Component\\ShowcaseResults'));
        $container->registerServiceProvider(new ComponentDispatcherFactory('\\Mlinnen\\Component\\ShowcaseResults'));
    }
}

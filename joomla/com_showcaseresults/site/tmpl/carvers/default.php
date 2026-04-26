<?php
/**
 * @package     ShowcaseResults
 * @subpackage  com_showcaseresults
 * @copyright   Copyright (C) 2026 mlinnen. All rights reserved.
 * @license     GNU General Public License version 2 or later
 */

defined('_JEXEC') or die;

use Joomla\CMS\Router\Route;

/**
 * Helper function to HTML-escape strings
 *
 * @param   string  $str  String to escape
 *
 * @return  string  Escaped string
 */
function escCarvers(string $str): string
{
    return htmlspecialchars($str, ENT_QUOTES | ENT_HTML5, 'UTF-8');
}

?>
<div class="showcaseresults-carvers">
    <?php if (isset($this->carversData['error'])): ?>

        <div class="cca-error">
            <p><?php echo escCarvers($this->carversData['error_message']); ?></p>
        </div>

    <?php elseif (!empty($this->carversData['no_event'])): ?>

        <div class="cca-year-selector">
            <h2>Carvers List</h2>
            <p>Select an event year to view the list of checked-in carvers:</p>
            <ul>
                <?php foreach ($this->carversData['available_events'] as $yr): ?>
                    <li>
                        <a href="<?php echo Route::_(
                            'index.php?option=com_showcaseresults&view=carvers&event=' . escCarvers($yr)
                        ); ?>">
                            <?php echo escCarvers($yr); ?>
                        </a>
                    </li>
                <?php endforeach; ?>
            </ul>
        </div>

    <?php elseif (!empty($this->carversData['carvers'])): ?>

        <h1><?php echo escCarvers($this->carversData['event_name']); ?> <?php echo escCarvers((string) $this->carversData['event_year']); ?></h1>

        <table class="cca-carvers-list">
            <thead>
                <tr>
                    <th>Carver ID</th>
                    <th>Carver Name</th>
                    <th>Division</th>
                </tr>
            </thead>
            <tbody>
                <?php foreach ($this->carversData['carvers'] as $carver): ?>
                    <tr>
                        <td><?php echo (int) $carver['carver_id']; ?></td>
                        <td>
                            <a href="<?php echo Route::_(
                                'index.php?option=com_showcaseresults&view=carver'
                                . '&carver_id=' . (int) $carver['carver_id']
                                . '&event=' . escCarvers((string) $this->carversData['event_year'])
                            ); ?>">
                                <?php echo escCarvers($carver['full_name']); ?>
                            </a>
                        </td>
                        <td><?php echo escCarvers($carver['division']); ?></td>
                    </tr>
                <?php endforeach; ?>
            </tbody>
        </table>

    <?php endif; ?>
</div>

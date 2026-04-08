<?php
/**
 * @package     ShowcaseResults
 * @subpackage  com_showcaseresults
 * @copyright   Copyright (C) 2026 mlinnen. All rights reserved.
 * @license     GNU General Public License version 2 or later
 */

defined('_JEXEC') or die;

/**
 * Helper function to convert place number to ordinal text
 *
 * @param   int  $n  Place number (1, 2, 3, etc.)
 *
 * @return  string  Ordinal text (1st, 2nd, 3rd, etc.)
 */
function ordinal(int $n): string
{
    if ($n <= 0) return '';
    
    $suffix = 'th';
    if ($n % 100 < 11 || $n % 100 > 13)
    {
        switch ($n % 10)
        {
            case 1: $suffix = 'st'; break;
            case 2: $suffix = 'nd'; break;
            case 3: $suffix = 'rd'; break;
        }
    }
    
    return $n . $suffix;
}

/**
 * Helper function to HTML-escape strings
 *
 * @param   string  $str  String to escape
 *
 * @return  string  Escaped string
 */
function esc(string $str): string
{
    return htmlspecialchars($str, ENT_QUOTES | ENT_HTML5, 'UTF-8');
}

?>
<div class="showcaseresults-carver">
    <?php if (isset($this->carverData['error'])): ?>
        <?php if ($this->carverData['error'] === 'no_parameters'): ?>
            <div class="cca-usage">
                <h2>How to use this page</h2>
                <p>Search for a carver's results using one of these methods:</p>
                <ul>
                    <li>By name: <code>?name=John+Doe</code></li>
                    <li>By name and year: <code>?name=John+Doe&amp;event=2024</code></li>
                    <li>By carver ID and year: <code>?carver_id=16&amp;event=2024</code></li>
                </ul>
            </div>
        <?php else: ?>
            <div class="cca-error">
                <p><?php echo esc($this->carverData['error_message']); ?></p>
            </div>
        <?php endif; ?>
    <?php elseif (!empty($this->carverData['results'])): ?>
        
        <?php
        // Determine subtitle based on query mode
        $app = Joomla\CMS\Factory::getApplication();
        $event = $app->input->getString('event', '');
        $subtitle = '';
        
        if (!empty($event) && count($this->carverData['results']) === 1)
        {
            $event = $this->carverData['results'][0];
            $subtitle = 'Results for ' . esc($event['event_name']) . ' ' . $event['event_year'];
        }
        else
        {
            $subtitle = 'Results across all events';
        }
        ?>
        
        <div class="cca-carver-header">
            <h1><?php echo esc($this->carverData['carver_name']); ?></h1>
            <p><?php echo $subtitle; ?></p>
        </div>
        
        <?php foreach ($this->carverData['results'] as $event): ?>
            <section class="cca-event-section">
                <h2><?php echo esc($event['event_name']); ?> <?php echo $event['event_year']; ?></h2>
                
                <?php if (!empty($event['special_prizes'])): ?>
                    <div class="cca-special-prizes">
                        <h3>Special Prizes</h3>
                        <table>
                            <thead>
                                <tr>
                                    <th>Award</th>
                                    <th>Prize</th>
                                    <th>Entry #</th>
                                </tr>
                            </thead>
                            <tbody>
                                <?php foreach ($event['special_prizes'] as $prize): ?>
                                    <tr>
                                        <td><?php echo esc($prize['award'] ?? ''); ?></td>
                                        <td><?php echo esc($prize['prize'] ?? ''); ?></td>
                                        <td>
                                            <?php if (!empty($prize['entry_number']) && $prize['entry_number'] > 0): ?>
                                                <?php echo esc($prize['entry_number']); ?>
                                            <?php endif; ?>
                                        </td>
                                    </tr>
                                <?php endforeach; ?>
                            </tbody>
                        </table>
                    </div>
                <?php endif; ?>
                
                <?php if (!empty($event['overall_results'])): ?>
                    <div class="cca-overall-results">
                        <h3>Overall Results</h3>
                        <table>
                            <thead>
                                <tr>
                                    <th>Category</th>
                                    <th>Place</th>
                                    <th>Entry #</th>
                                </tr>
                            </thead>
                            <tbody>
                                <?php foreach ($event['overall_results'] as $category): ?>
                                    <?php foreach ($category['places'] as $place): ?>
                                        <tr>
                                            <td><?php echo esc($category['category']); ?></td>
                                            <td><?php echo ordinal($place['place'] ?? 0); ?></td>
                                            <td>
                                                <?php if (!empty($place['entry_number']) && $place['entry_number'] > 0): ?>
                                                    <?php echo esc($place['entry_number']); ?>
                                                <?php endif; ?>
                                            </td>
                                        </tr>
                                    <?php endforeach; ?>
                                <?php endforeach; ?>
                            </tbody>
                        </table>
                    </div>
                <?php endif; ?>
                
                <?php if (!empty($event['division_results'])): ?>
                    <div class="cca-division-results">
                        <h3>Division Results</h3>
                        <?php foreach ($event['division_results'] as $division): ?>
                            <h4><?php echo esc($division['division']); ?></h4>
                            <table>
                                <thead>
                                    <tr>
                                        <th>Category</th>
                                        <th>Style</th>
                                        <th>Place</th>
                                        <th>Entry #</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <?php foreach ($division['categories'] as $category): ?>
                                        <?php foreach ($category['places'] as $place): ?>
                                            <tr>
                                                <td><?php echo esc($category['name']); ?></td>
                                                <td>
                                                    <?php 
                                                    if ($category['style'] === 'N') {
                                                        echo 'Natural';
                                                    } elseif ($category['style'] === 'P') {
                                                        echo 'Painted';
                                                    }
                                                    ?>
                                                </td>
                                                <td><?php echo ordinal($place['place'] ?? 0); ?></td>
                                                <td>
                                                    <?php if (!empty($place['entry_number']) && $place['entry_number'] > 0): ?>
                                                        <?php echo esc($place['entry_number']); ?>
                                                    <?php endif; ?>
                                                </td>
                                            </tr>
                                        <?php endforeach; ?>
                                    <?php endforeach; ?>
                                </tbody>
                            </table>
                        <?php endforeach; ?>
                    </div>
                <?php endif; ?>
                
            </section>
        <?php endforeach; ?>
        
    <?php endif; ?>
</div>

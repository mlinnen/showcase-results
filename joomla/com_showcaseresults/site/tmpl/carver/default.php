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

?>
<div class="showcaseresults-carver">
    <?php if (isset($this->carverData['error'])): ?>
        <div class="alert alert-warning">
            <p><strong>Error:</strong> <?php echo htmlspecialchars($this->carverData['error']); ?></p>
        </div>
    <?php elseif (isset($this->carverData['found']) && $this->carverData['found'] === false): ?>
        <p>No results found for the specified carver.</p>
    <?php elseif (!empty($this->carverData['results'])): ?>
        
        <?php
        // Determine subtitle based on query mode
        $app = Joomla\CMS\Factory::getApplication();
        $year = $app->input->getInt('year', 0);
        $subtitle = '';
        
        if ($year > 0 && count($this->carverData['results']) === 1)
        {
            $event = $this->carverData['results'][0];
            $subtitle = 'Results for ' . htmlspecialchars($event['event_name']) . ' ' . $event['event_year'];
        }
        else
        {
            $subtitle = 'Results across all events';
        }
        ?>
        
        <div class="cca-carver-header">
            <h1><?php echo htmlspecialchars($this->carverData['carver_name']); ?></h1>
            <p><?php echo $subtitle; ?></p>
        </div>
        
        <?php foreach ($this->carverData['results'] as $event): ?>
            <section class="cca-event-section">
                <h2><?php echo htmlspecialchars($event['event_name']); ?> <?php echo $event['event_year']; ?></h2>
                
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
                                        <td><?php echo htmlspecialchars($prize['award'] ?? ''); ?></td>
                                        <td><?php echo htmlspecialchars($prize['prize'] ?? ''); ?></td>
                                        <td>
                                            <?php if (!empty($prize['entry_number']) && $prize['entry_number'] > 0): ?>
                                                <?php echo htmlspecialchars($prize['entry_number']); ?>
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
                                            <td><?php echo htmlspecialchars($category['category']); ?></td>
                                            <td><?php echo ordinal($place['place'] ?? 0); ?></td>
                                            <td>
                                                <?php if (!empty($place['entry_number']) && $place['entry_number'] > 0): ?>
                                                    <?php echo htmlspecialchars($place['entry_number']); ?>
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
                            <h4><?php echo htmlspecialchars($division['division']); ?></h4>
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
                                                <td><?php echo htmlspecialchars($category['name']); ?></td>
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
                                                        <?php echo htmlspecialchars($place['entry_number']); ?>
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

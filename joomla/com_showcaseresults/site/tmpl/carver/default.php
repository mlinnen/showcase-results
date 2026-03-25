<?php
/**
 * @package     ShowcaseResults
 * @subpackage  com_showcaseresults
 * @copyright   Copyright (C) 2026 mlinnen. All rights reserved.
 * @license     GNU General Public License version 2 or later
 */

defined('_JEXEC') or die;

// Placeholder template for carver results view
// This will be enhanced with actual rendering logic in issue #11

?>
<div class="showcaseresults-carver">
    <h2>Showcase Results - Carver View (placeholder)</h2>
    
    <?php if (!empty($this->name)): ?>
        <p><strong>Name:</strong> <?php echo htmlspecialchars($this->name); ?></p>
    <?php endif; ?>
    
    <?php if (!empty($this->carver_id)): ?>
        <p><strong>Carver ID:</strong> <?php echo (int) $this->carver_id; ?></p>
    <?php endif; ?>
    
    <?php if (!empty($this->year)): ?>
        <p><strong>Year:</strong> <?php echo (int) $this->year; ?></p>
    <?php endif; ?>
    
    <?php if (!empty($this->carver_id) && empty($this->year)): ?>
        <div class="alert alert-warning">
            <p><strong>Note:</strong> carver_id requires a year parameter (error handling comes in issue #12)</p>
        </div>
    <?php endif; ?>
    
    <p class="text-muted">Full rendering logic will be implemented in issue #11</p>
</div>

<?php
/**
 * @package     ShowcaseResults
 * @subpackage  com_showcaseresults
 * @copyright   Copyright (C) 2026 mlinnen. All rights reserved.
 * @license     GNU General Public License version 2 or later
 */

defined('_JEXEC') or die;

?>
<div class="showcaseresults-carver">
    <h2>Showcase Results - Carver View</h2>
    
    <?php if (isset($this->carverData['error'])): ?>
        <div class="alert alert-warning">
            <p><strong>Error:</strong> <?php echo htmlspecialchars($this->carverData['error']); ?></p>
        </div>
    <?php endif; ?>
    
    <?php if (!empty($this->carverData['carver_name'])): ?>
        <h3><?php echo htmlspecialchars($this->carverData['carver_name']); ?></h3>
    <?php endif; ?>
    
    <?php if (isset($this->carverData['found']) && $this->carverData['found'] === false): ?>
        <p class="text-muted">No results found for the specified carver.</p>
    <?php elseif (!empty($this->carverData['results'])): ?>
        <div class="results-data">
            <h4>Data Loaded (Raw Output — Rendering Enhancement in Issue #11)</h4>
            <pre><?php var_dump($this->carverData); ?></pre>
        </div>
    <?php endif; ?>
</div>

$(function () {
    'use strict';

    function bindPreview($textarea) {
        var previewTarget = $textarea.data('preview-target');
        if (!previewTarget) {
            return;
        }

        var $preview = $('#' + previewTarget);
        if (!$preview.length) {
            return;
        }

        var render = function () {
            var html = $textarea.val();
            if (html === undefined || html === null) {
                html = '';
            }

            renderHtmlPreview($preview, html);
        };

        $textarea.on('input', render);
        render();
    }

    $('[data-template-editor="true"]').each(function () {
        bindPreview($(this));
    });
});

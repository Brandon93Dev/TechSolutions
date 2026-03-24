/* 
    Created a common.js file as i reused a lot of code and createdx some duplicate code that does the same thing in many places in the project
    this now provides a centralised place where reusable js can be created.
*/

function pick(obj, camelKey, pascalKey, fallback) {
    if (!obj) {
        return fallback;
    }

    var camelValue = obj[camelKey];
    if (camelValue !== undefined && camelValue !== null) {
        return camelValue;
    }

    var pascalValue = obj[pascalKey];
    if (pascalValue !== undefined && pascalValue !== null) {
        return pascalValue;
    }

    return fallback;
};

//Safety fallback to ensure that data fetched from backend does not cause table or elemtn rendering to fail
// in other words removes unsafe html elements
function htmlEncode(text) {
    var safeText = text;
    if (safeText === undefined || safeText === null) {
        safeText = '';
    }

    return $('<span>').text(safeText).html();
};

function attrEncode(text) {
    var value = text;
    if (value === undefined || value === null) {
        value = '';
    }

    value = value.replace(/&/g, '&amp;');
    value = value.replace(/"/g, '&quot;');
    value = value.replace(/'/g, '&#39;');
    value = value.replace(/</g, '&lt;');
    value = value.replace(/>/g, '&gt;');

    return value;
};

function escapeHtml(text) {
    return htmlEncode(text);
};


// for html email templates, if we want to render the content this is used, 
// this safely renders it in a iframe that cannot affect content outside of its scope
function renderHtmlPreview(target, html) {
    var $target = target && target.jquery ? target : $(target);
    if (!$target.length) {
        return;
    }

    var value = html;
    if (value === undefined || value === null) {
        value = '';
    }

    if ($target.is('iframe')) {
        $target.attr('srcdoc', value);
        return;
    }

    $target.html(value);
};

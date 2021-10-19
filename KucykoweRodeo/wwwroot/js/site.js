// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

"use strict";

function addSuggestions(fieldSelector, apiUrl, onSuccess) {
  function split(val) {
    return val.split(/,\s*/);
  }

  $(fieldSelector)
    // don't navigate away from the field on tab when selecting an item
    .on("keydown",
      function (event) {
        if (event.keyCode === $.ui.keyCode.TAB &&
          $(this).autocomplete("instance").menu.active) {
          event.preventDefault();
        }
      })
    .autocomplete({
      minLength: 0,
      source: function (request, response) {
        $.ajax({
          url: apiUrl,
          data: { "query": request.term },
          dataType: "json",
          success: function(data) {
            response(onSuccess ? onSuccess(data) : data);
          }
        });
      },
      focus: function () {
        // prevent value inserted on focus
        return false;
      },
      select: function (event, ui) {
        var query = split(this.value);
        // remove the current input
        query.pop();
        // add the selected item
        query.push(ui.item.value);
        // add placeholder to get the comma-and-space at the end
        query.push("");
        this.value = query.join(", ");
        return false;
      }
    });
}

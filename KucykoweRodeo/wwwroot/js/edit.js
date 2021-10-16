"use strict";

function addSuggestions(fieldSelector, apiUrl) {
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
          success: function (data) {
            response(data);
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

$(function () {
  addSuggestions("textarea[name=tags]", "/Tags/Suggest");
});

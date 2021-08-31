"use strict";

$(function () {
  function split(val) {
    return val.split(/,\s*/);
  }

  function extractLast(term) {
    return split(term).pop();
  }

  $("input[name=query]")
    // don't navigate away from the field on tab when selecting an item
    .on("keydown", function (event) {
      if (event.keyCode === $.ui.keyCode.TAB &&
        $(this).autocomplete("instance").menu.active) {
        event.preventDefault();
      }
    })
    .autocomplete({
      minLength: 0,
      source: function (request, response) {
        $.ajax({
          url: "/Search/Suggest",
          data: { "query": request.term },
          dataType: "json",
          success: function (data) {
            response(
              data.tags.map(function (tag) {
                return {
                  "caption": tag,
                  "value": tag
                }
              })
              .concat(data.categories.map(function (category) {
                return {
                  "caption": "Kategoria: " + category.name,
                  "value": "c:" + category.id
                }
              }))
              .concat(data.authors.map(function (author) {
                return {
                  "caption": "Autor: " + author.name,
                  "value": "a:" + author.id
                }
              })));
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
    })
    .autocomplete("instance")._renderItem = function (ul, item) {
      return $("<li>")
        .append("<div>" + item.caption + "</div>")
        .appendTo(ul);
    };
});

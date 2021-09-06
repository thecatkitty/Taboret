"use strict";

$(function () {
  function split(val) {
    return val.split(/,\s*/);
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
              data.features.map(function (feature) {
                var prefixCaptions = {
                  "a:": "Autor: ",
                  "c:": "Kategoria: "
                };
                var prefix = feature.value.substring(0, 2);

                return {
                  "prefix": prefixCaptions[prefix] || "",
                  "caption": feature.caption,
                  "value": feature.value
                }
              }));
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
      var div = $("<div>")
        .append("<span class='text-muted'>" + item.prefix + "</span>")
        .append("<span>" + item.caption + "</span>");
      return $("<li>")
        .append(div)
        .appendTo(ul);
    };
});

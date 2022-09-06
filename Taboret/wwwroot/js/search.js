"use strict";

$(function () {
  function boldQuery(str, query) {
    var n = str.toUpperCase();
    var q = query.toUpperCase();
    var x = n.indexOf(q);
    if (!q || x === -1) {
      return str; // bail early
    }
    var l = q.length;
    return str.substr(0, x) + "<b>" + str.substr(x, l) + "</b>" + str.substr(x + l);
  }

  var selector = "input[name=query]";
  var articles = [];

  function onSuccess(data) {
    articles = data.articles;

    if (data.features.length === 0)
      return [
        {
          "prefix": "",
          "caption": "",
          "value": ""
        }
      ];

    return data.features.map(function (feature) {
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
    });
  }

  addSuggestions(selector, "/Search/Suggest", onSuccess);

  var input = $(selector);
  input.autocomplete("instance")._renderItem = function (ul, item) {
    var div = $("<div>")
      .append("<span class='text-muted'>" + item.prefix + "</span>")
      .append("<span>" + item.caption + "</span>");
    return $("<li class='col-md-6'>")
      .append(div)
      .appendTo(ul);
  };

  input.autocomplete("instance")._renderMenu = function (ul, items) {
    var that = this;
    var articlesLi = $("<li class='articles float-md-right col-md-6 p-0'>");
    var articlesUl = $("<div class='list-group list-group-flush'>").appendTo(articlesLi);
    articles.forEach(article => {
      var query = input.val();
      var div = $("<div class='list-group-item list-group-item-action'>");
      div.append("<a href='" + article.url + "' class='stretched-link d-block'>" + boldQuery(article.title, query) + "</a>");
      if (article.subject) div.append("<small class='d-block'>" + boldQuery(article.subject, query) + "</small>");
      div.append("<small class='text-muted d-block'>" + article.authors.join(", ") + " w " + article.issue + "</small>");
      articlesUl.append(div);
    });

    $(ul).append(articlesLi);
    $(ul).append("<li class='disabled font-weight-bold col-md-6'>Tagi:</li>");

    $.each(items, function (index, item) {
      that._renderItemData(ul, item);
    });
  };
});

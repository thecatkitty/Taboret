"use strict";

$(function () {
  addSuggestions("input[name=coverAuthors]", "/Authors/Suggest");
  addSuggestions("input[name=authors]", "/Authors/Suggest");
  addSuggestions("textarea[name=tags]", "/Tags/Suggest");
});

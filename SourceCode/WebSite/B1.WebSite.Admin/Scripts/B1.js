/*
FileName: B1.js
var list = new B1List("idList", {showCheckBox: true});
*/

var B1List = function (table_id, options) {
    // Store "this" in local variable so you can close over current "this" object
    var thisObj = this;

    this.id = table_id;

    var rowChecked = function (chk) {
        var tr = $(chk).parent().parent();
        if (chk.checked && !tr.hasClass('checked')) tr.addClass('checked');
        else if (tr.hasClass('checked')) tr.removeClass('checked');
    }

    var checkClicked = function () {
        var tr = $(this).parent().parent();
        var chk = this;

        // Header checkbox clicked
        if ($(this).parent()[0].tagName.toUpperCase() == 'TH') {
            $("#" + thisObj.id).find('td.checkbox > :input').each(function () {
                this.checked = chk.checked;
                rowChecked(this);
            });
        } else {
            rowChecked(this);
        }
    };

    // Add the checkboxes to every row
    $("#" + this.id).find('td.checkbox, th.checkbox').each(function () {
        $(this).css({ "text-align": "center" });
        $(this).html("<input type='checkbox' />");
        $(this).children().click(checkClicked);
    });

    //?? Add the row number to every row

    // Set the hoverOver event
    $("#" + this.id + " tbody tr").hover(
        function () { $(this).addClass("hover"); },
        function () { $(this).removeClass("hover"); });

    // Clicking on TBODY TR (row) - does the focus/selection/check etc.
    $("#" + this.id + " tbody tr").click(function (e) {
        // Select the row only if the clicking did not happen on any input element
        if (e.target.tagName != "INPUT") {
            // JQuery runs this function in the clicked TR element context - so this
            // represents the clicked TR
            thisObj.selectRow(this);
        }
    });

    if (options.onselected) {
        $("#" + this.id).click(function (e) {
            if (e.target.tagName == "TD") {
                options.onselected(e.target);
            }
        });
    }

    // If the table is editable
    if (options.editable) {
        // Add a hidden input text box
        //?? For some reason: the display:none and then show() hide() mechanism is NOT working - the offset of the input element
        // is getting doubled every time
        //        var style = "font-family:TrebuchetMS;font-size:9pt;background-color:white;border: solid 1px black;color:0000FF;" +
        //                    "text-align: left;position: absolute;display:none; z-index: 1000;";
        var style = "font-family:TrebuchetMS;font-size:9pt;background-color:white;border: solid 1px black;color:0000FF;" +
                    "text-align: left;position: absolute;visibility:hidden; z-index: 1000;";
        this.inp = $('<input style="' + style + '" type="text" value="" />').appendTo('body');
        this.inp.blur(function () {
            thisObj.updateValue();
            //?? thisObj.inp.hide();
            thisObj.inp.css("visibility", "hidden");
        });

        // Add the keyboard interaction
        this.inp.keydown(function (e) {
            return thisObj.editKeyDown(e);
        });

        // Move the edit box to the clicked editable cell
        $("#" + this.id).click(function (e) {
            if (e.target.tagName == "TD" && $(e.target).hasClass("editable")) {
                thisObj.makeCellEditable(e.target);
            }
        });

        // Attach add new row function if "add-item" is defined
        $("#" + this.id).find('.add-item').click(function (e) {
            thisObj.addRow();
        });

    }
}

B1List.prototype.getRow = function (trOrIndex) {
    return (typeof trOrIndex !== "number") ?
        trOrIndex :
        document.getElementById(this.id).rows[trOrIndex + 1]; //?? Assuming one header row. If more fix it here.
}

B1List.prototype.addRow = function () {
    var tr = $("#" + this.id).find('tr.row-template');
    var newtr = $(tr)
                    .clone(true)
                    .removeClass('row-template')
                    .appendTo("#" + this.id + " tbody")
                    .show();
    this.makeCellEditable(newtr.find('td.editable')[0]);
}

B1List.prototype.selectRow = function (trOrIndex) {
    var tr = this.getRow(trOrIndex);

    // Change focus to this row
    this.focusRow(tr); //?? donno yet - if we need to do this.

    // Check if multiple selection are allowed
    // showCheckBox

    /*    if ($(tr).hasClass("check")) {
    $(tr).removeClass("check");
    } else {
    $(tr).addClass("check");
    }
    */
}

B1List.prototype.focusRow = function (trOrIndex) {
    var tr = this.getRow(trOrIndex);

    // Un-focus the previously focused row
    if (this.focusedRow != null) $(this.focusedRow).removeClass("focus");
    this.focusedRow = tr;
    $(this.focusedRow).addClass("focus");
}

B1List.prototype.updateValue = function () {
    if (this.td == null) return;

    //    var inputValue = this.td.children('input').val();
    // Mark row edited when a value has changed //?? Mark also a cell so that only the changed cells are sent for performance //??
    if (typeof this.td[0].oldValue != 'undefined' && this.td[0].oldValue != this.inp.val() && !this.td.hasClass('edited')) {
        if (!this.td.parent().hasClass('edited')) this.td.parent().addClass('edited');
        this.td.addClass('edited');
    }

    this.td.html(this.inp.val());
    //?? Ajax back to the home server
}

var r20 = /%20/g;
B1List.prototype.getPostData = function () {
    // If a previous edit box is visible then update that value
    this.updateValue();

    // Hint for IE: IE caches some Ajax request so please explicitly make sure to take care of IE issue. You can use
    // "post" method, or add a dummy timestamp parameter to the URL, if using ajax - use "cache: false" as option.

    var s = [],
			add = function (key, value) {
			    // If value is a function, invoke it and return its value
			    value = jQuery.isFunction(value) ? value() : value;
			    s[s.length] = encodeURIComponent(key) + "=" + encodeURIComponent(value);
			};

    /*
    Data string (URL encoding is done to key and value):
    ID=1&Name=Hello&Zip=1111
    &ID=2&Name=Hello22&Zip=2222
    */

    // Get header row
    var headRow = $("#" + this.id + " thead tr");
    var keys = headRow.find('th.key'); //?? or td.key //?? TR can have TH or TD

    var editedRows = $("#" + this.id).find('tr.edited');
    if (editedRows.length > 0) {
        editedRows.each(function () {
            var tr = this;
            // Add the keys for the changed rows
            keys.each(function () {
                add(this.innerHTML, tr.cells[this.cellIndex].innerHTML);
            });

            // Add the values for the changed rows
            headRow.find('th:not(.key)').each(function () {
                add(this.innerHTML, tr.cells[this.cellIndex].innerHTML);
            });
        });
    }

    return s.join("&").replace(r20, "+");
}

B1List.prototype.getCheckedData = function (separator) {
    separator = separator || "&";

    var s = [],
			add = function (key, value) {
			    // If value is a function, invoke it and return its value
			    value = jQuery.isFunction(value) ? value() : value;
			    s[s.length] = encodeURIComponent(key) + "=" + encodeURIComponent(value);
			};

    /*
    Data string (URL encoding is done to key and value):
    ID=1&ID=2
    */

    // Get header row
    var headRow = $("#" + this.id + " thead tr");
    var keys = headRow.find('th.key'); //?? or td.key //?? TR can have TH or TD

    // Get all the checked rows
    var checkedRows = $("#" + this.id).find('tr.checked');
    if (checkedRows.length > 0) {
        checkedRows.each(function () {
            var tr = this;
            // Add the keys for the checked rows
            keys.each(function () {
                add(this.innerHTML, tr.cells[this.cellIndex].innerHTML);
            });
        });
    }

    return s.join(separator).replace(r20, "+");
}

B1List.prototype.makeCellEditable = function (td) {

    // If a previous edit box is visible then update that value
    this.updateValue();

    // If the passed object has nodeType that means it is pure DOM element, jQuerify it
    this.td = td = $(td);

    // Check if the oldValue custom property exists (DO it on the real DOM element)
    if (typeof td[0].oldValue == 'undefined')
        td[0].oldValue = td.text();

    this.inp.val(td.text());
    this.inp.offset(td.offset());
    this.inp.width(td.width());
    this.inp.height(td.height());
    //?? this.inp.show();
    this.inp.css("visibility", "visible");
    this.inp.select();
    this.inp.focus();

    /*
    this.td.css('padding', 0);
    this.td.html("<input type='text' class='inp' style='width: 100%; height: 100%' value='" + this.td.text() + "' />");
    var thisObj = this;
    this.td.children('input').select().focus().keydown(function (e) {
    return thisObj.editKeyDown(e);
    });*/
}


B1List.prototype.editKeyDown = function (e) {
    // TAB to tab through cell
    if (e.which == 9 || e.which == 13) {
        var td = e.shiftKey ?
            this.getPrevEditableCell(this.td) :
            this.getNextEditableCell(this.td);
        if (td != null) this.makeCellEditable(td);
        else if (!e.shiftKey) { // Add new row on Enter or Tab at the last row
            //?? If the last row is empty then dont add new row            
            this.addRow();
        }
        return false;
    }
}

B1List.prototype.getPrevEditableCell = function (td) {
    var tdPrev = td.prev('td.editable');
    if (tdPrev.length == 0) {
        var trPrev = td.parent().prev('tr');
        if (trPrev.length != 0 && trPrev[0].className != 'row-template') {
            tdPrev = trPrev.children('td.editable:last')
        }
    }
    return tdPrev.length != 0 ? tdPrev : null;
}

B1List.prototype.getNextEditableCell = function (td) {
    var tdNext = td.next('td.editable');
    if (tdNext.length == 0) {
        var trNext = td.parent().next('tr');
        if (trNext.length != 0) {
            tdNext = trNext.children('td.editable:first')
        }
    }
    return tdNext.length != 0 ? tdNext : null;
}

var HtmlTable = function (divSelector, options, data) {
    this.targetDiv = $(divSelector);
    this.options = options;
    this.data = data;

    // Render the table
    this.targetDiv.html(this._renderTable());
}

HtmlTable.prototype._renderTable = function () {
    var html =
        "<table id='idList' tabindex='0'>" +
            "<thead>" + this._renderHeaderRow() + "</thead>" +
            "<tbody>" +
                jQuery.map(this.data.rows, bind_this(this, this._renderRow)).join("") +
            "</tbody>" +
        "</table>";

    alert(html);
    return html;
};

HtmlTable.prototype._renderHeaderRow = function () {
    var thisObj = this; // Closing over "this" for the anonymous fn
    var html =
        "<tr>" +
            (this.options.showCheckBox ? "<td><input type='checkbox' /></td>" : "") +
            (this.options.showRowNumber ? "<td>#</td>" : "") +
            jQuery.map(this.options.displayColumns, function (columnName, index) {
                return "<th>" + thisObj.options.displayTitles[index] + "</th>";
            }).join("") +
        "</tr>";
    return html;
};

HtmlTable.prototype._renderRow = function (row, index) {
    var html =
        "<tr>" +
            (this.options.showCheckBox ? "<td><input type='checkbox' /></td>" : "") +
            (this.options.showRowNumber ? "<td>" + (index + 1) + "</td>" : "") +
            jQuery.map(this.options.displayColumns, function (columnName) {
                return "<td>" + row[columnName] + "</td>";
            }).join("") +
        "</tr>";
    return html;
};


/////////////////// /////////////////// /////////////////
/////////////////// Utility functions ///////////////////
/////////////////// /////////////////// /////////////////

//
// jQuery functions such as map changes the calling context to the matched elements
// so passing them member fn needs to be closed over with the caller "this"
//
// This function creates a closure each time a class "member" fn is used
// as a callback function. When you pass around callback fn, "this" referes
// to the object it was called on - not the object whose member it is. JavaScript
// closure can be used to close over "this" (which make it a named variable at that
// time) inside an anonymous fn. It boils down to the following code:
//
//      var obj = this;
//      jQuery.map( arr, function () { obj.memberfn(arguments) });
//
// bind_this function creates a clojure which takes 5 arguments so you can use it for
// callback which need upto the 5 arguments. you can use it for less then 5 args too.
//
function bind_this(obj, fn) {
    return function (arg1, arg2, arg3, arg4, arg5) { return fn.call(obj, arg1, arg2, arg3, arg4, arg5); }
}

/*
// Moving animation - Following functions creates a moving animation from one element to another
var dragMe = null;
function selectAnimation(src, dest, dropfn) {
src = $(src);
dest = $(dest);

if (dragMe == null) {
dragMe = $("<div/>", {
"style": "position: absolute; display: none; z-index: 1000;background: #F39814; color: white; margin: 3px 3px 3px 0px; padding: 1px; float: left; width: 200px; height: 40px; font-size: small; text-align: left; word-wrap: break-word;"
}).appendTo("body");
}

srcPosition = src.offset();
destPosition = dest.offset();

dragMe.html(src.html());
dragMe.offset({ top: srcPosition.top, left: srcPosition.left });
dragMe.width(src.width);
dragMe.height(src.height);
dragMe.show();
dragMe.animate({ "left": destPosition.left, "top": destPosition.top }, 300, "linear",
function () {
dropfn(dragMe.html());
dragMe.hide();
});
}
*/

var SelectionBar = function (container, title) {
    // Page container where the selected items are added.
    this.container = $(container);

    // Items which are selected
    this.items = new Array();

    // Item for showing the title
    this.titleItem = $("<div/>", {
        "style": "clear: both; margin-left: 10px;"
    }).appendTo(this.container);
    this.titleItem.html("<h2>" + title + "</h2>");

    // Div which is used for the selection animation
    this.dragMe = $("<div/>", {
        "style": "position: absolute; display: none; z-index: 1000;background: #F39814; color: white; margin: 3px 3px 3px 0px; padding: 1px; float: left; width: 200px; height: 40px; font-size: small; text-align: left; word-wrap: break-word;"
    }).appendTo("body");
}

SelectionBar.prototype.Add = function (fromSrc, width, title, cb) {
    fromSrc = $(fromSrc);
    width = width || 100;

    // Add a new item where the selection will be animated to
    this.items[this.items.length] = $("<div/>", {
        "style": "float: left; margin-left: 10px;"
    }).insertBefore(this.titleItem);
    /// <reference path="jquery.validate.min.js" />

    var index = this.items.length - 1;
    var item = this.items[index];

    var srcPosition = fromSrc.offset();
    var destPosition = item.offset();

    this.dragMe.html(fromSrc.html());
    this.dragMe.offset({ left: srcPosition.left, top: srcPosition.top });
    this.dragMe.width(fromSrc.width);
    this.dragMe.height(fromSrc.height);
    this.dragMe.show();
    var thisObj = this;
    this.dragMe.animate({ "left": destPosition.left, "top": destPosition.top }, 300, "linear",
        function () {
            item.html("<table border='0' cellspacing='0' cellpadding='0'>" +
                "<tr>" +
                    "<td style='font-size: 0px; line-height: 0%; width: 0px; border-top: 10px solid #606060; border-left: 10px solid #f6f6f6; border-bottom: 10px solid #606060;'></td>" +
                    "<td class='item-data' style='background-color: #606060; color: White; font-weight: bold; text-align: center; width: " + width + "px;'>" + thisObj.dragMe.html() + "</td>" +
                    "<td style='font-size: 0px; line-height: 0%; width: 0px; border-top: 10px solid #f6f6f6; border-left: 10px solid #606060; border-bottom: 10px solid #f6f6f6;'></td>" +
                "</tr>" +
            "</table>");

            // Add click behavior to the selection if a callback fn is provided
            if (cb) {
                item.find("td.item-data")
                    .click(function () { thisObj.SelectionClicked(cb, index) })
                    .mouseover(function () { $(this).css("cursor", "pointer"); });
            }

            thisObj.dragMe.hide();

            thisObj.titleItem.html("<h2>" + title + "</h2>");
        });
}

SelectionBar.prototype.Get = function (index) {
    return this.items[index].find("td.item-data").html();
}

SelectionBar.prototype.SelectionClicked = function (cb, index) {
    // If the callback returns true then remove the remaining selection
    if (cb()) {
        var len = this.items.length;
        this.items = jQuery.grep(this.items, function (item, i) {
            if (i >= index && i < len) {
                item.remove();
                return false;
            }
            return true;
        });
    }
}

///*
// new ScreenSwapper('#containerDiv');
var ScreenSwapper = function (container, width) {
    this.container = $(container);
    this.width = width;
    this.container.css({ 'position': 'absolute', 'width': width });
}

// Based on what is currently displaying - either the screen slide to the left
// or right.
ScreenSwapper.prototype.show = function (screen, leftToRight) {
    // If this screen is already current visible screen then dont do anything
    if (this.screen != null && this.screen == screen) return;

    leftToRight = leftToRight | (this.current == null);
    var containerPosition = this.container.offset();
    if (this.current != null) {
        var old = this.current;
        old.animate({ "left": -this.width, "top": containerPosition.top },
            { duration: 1000,
                complete: bind_this(this, function () { old.hide(); })
            });
    }

    this.screen = screen;
    this.current = $(screen);
    this.current.css('width', this.width);

    if (leftToRight) {
        this.current.show();
        this.current.offset({ top: containerPosition.top, left: -this.width });
        this.current.animate({ "left": containerPosition.left + 5, "top": containerPosition.top }, { duration: 1000 })
    } else {
        this.current.show();
        this.current.offset({ left: this.width, top: containerPosition.top });
        this.current.animate({ "left": containerPosition.left + 5, "top": containerPosition.top }, { duration: 1000 })
    }
}

// DialogForm with feedback shown //?? Currently dialog options are hardcoded
// submitFn can return {error: true, Msg: error message or success message}  - Optionally it can have buttons
var DialogForm = function (divSelector) {
    var thisObj = this;
    this.formScreen = $($(divSelector).find(".edit-form")[0]);       // Original forms screen
    this.feedbackScreen = $("<div id='feedbackScreen'></div>").appendTo(divSelector);    // Feedback screen - success or failure messages is shown here
    this.dialogScreen = $(divSelector);                              // Dialog screen - this is where forms, feedbacks screens are rotated.

    //?? Temporary to test the overlay
    this.divSelector = divSelector;

    // Close button
    this.closeButton = {
        text: "Close",
        click: function () { $(this).dialog("close"); }
    };

    // Retry button
    this.retryButton = {
        text: "Retry",
        click: function () {
            thisObj.dialogScreen.dialog("option", "buttons", [thisObj.saveButton, thisObj.closeButton]);
            thisObj.feedbackScreen.hide();
            thisObj.formScreen.show();
        }
    };

    // showEmpty
    // showResetToDefault
    // ShowLoadingPage - loading with the whole dialog form disabled true/false
    // ShowFeedbackPage - Pass message which is shown. two flavor: Close, Retry/GiveUp
    //   - Retry shows the form again with existing entries
}

// Shows dialog form
//?? Select all form fields - text boxes, radio buttons, textarea, checkbxes etc and empty its values
//?? this.formScreen
DialogForm.prototype.show = function (ajaxUrl, datafn, onSuccessFn, width) {
    var thisObj = this;
    width = width || 600;

    this.feedbackScreen.hide();
    this.formScreen.show();

    // Clear out the form
    this.formScreen.find("form")[0].reset();

    this.saveButton = {
        text: "Save",
        click: function () {
            if (thisObj.validate()) {

                //?? Disable editing of the form with an overlay screen
                overlayScreen();

                $.ajax({
                    url: ajaxUrl,
                    type: "post",
                    format: "html",
                    data: datafn(),
                    success: function (result) {
                        thisObj.showFeedback({ success: true, responseText: result });
                        onSuccessFn(result);
                    },
                    error: function (xhr, status, error) {
                        thisObj.showFeedback({ success: false, responseText: xhr.responseText });
                    }
                });
            }
        }
    };

    // Load the dialog box
    this.dialogScreen.dialog({ width: width, height: 400, modal: true, buttons: [this.saveButton, this.closeButton] });
}

DialogForm.prototype.validate = function () {
    var validated = true;
    var focusToInput = null;

    //?? check for checkbox, radio, text, textarea, button etc.
    var inputs = this.formScreen.find(":input.required");
    for (var i = 0; i < inputs.length; i++) {
        if (this.isEmpty(inputs[i])) {
            if (!focusToInput) focusToInput = $(inputs[i]);
            this.markRequired(inputs[i]);
            validated = false;
        }
    }

    if (!validated) focusToInput.focus();
    return validated;
}

DialogForm.prototype.isEmpty = function (input) {
    // If checkbox field is required then it must be check
    if (input.type == "checkbox" && !input.checked) return true;

    // if text field is required then it must have value
    if (input.type == "text" && input.value.length == 0) return true;
}

DialogForm.prototype.markRequired = function (input) {
    $(input).prev().show();
    $(input).bind('change', function (event) {
        $(this).prev().hide();
        $(this).unbind(event);
    });
}

// show feedback screen
DialogForm.prototype.showFeedback = function (result) {
    var thisObj = this;

    this.formScreen.hide();
    this.feedbackScreen.html(result.responseText);
    this.feedbackScreen.show();

    // Change the buttons
    this.dialogScreen.dialog("option", "buttons",
        result.success ? [this.closeButton] : [this.retryButton, this.closeButton]);
}

// This function overlays the given screen with a given message
function overlayScreen(selector, message) {
    var overlayScreen = $("<div>Loading...</div>")
            .prependTo(this.divSelector)
            .attr("style", "overflow: hidden; display: block; position: fixed; width: 500px; height: 540px; outline-color: invert; outline-style: none; outline-width: 0pt; top: 0pt; left: 442px; z-index: 1002;");

}

/*
function calcOffset(o, offsetName) {
var totalOffset = 0;
do {
totalOffset += eval('o.' + offsetName);
o = o.offsetParent;
} while (o != null);

return totalOffset;
}
*/

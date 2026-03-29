let CheckConfirm = true;
let ActiveForm = "";
//var ActiveButton = undefined;
function SetConfirm() {
    CheckConfirm = true;
    SetValueSimpl('ConfirmFormBtn', '');
}
 
(function ($) {
    var data_click = "unobtrusiveAjaxClick",
        data_target = "unobtrusiveAjaxClickTarget",
        data_validation = "unobtrusiveValidation";

    function getFunction(code, argNames) {
        var fn = window, parts = (code || "").split(".");
        while (fn && parts.length) {
            fn = fn[parts.shift()];
        }
        if (typeof (fn) === "function") {
            return fn;
        }
        argNames.push(code);
        return Function.constructor.apply(null, argNames);
    }

    function isMethodProxySafe(method) {
        return method === "GET" || method === "POST";
    }

    function asyncOnBeforeSend(xhr, method) {
        if (!isMethodProxySafe(method)) {
            xhr.setRequestHeader("X-HTTP-Method-Override", method);
        }
    }

    function asyncOnSuccess(element, data, contentType) {
        var mode;

        if (contentType.indexOf("application/x-javascript") !== -1) {  // jQuery already executes JavaScript for us
            return;
        }

        mode = (element.getAttribute("data-ajax-mode") || "").toUpperCase();
        $(element.getAttribute("data-ajax-update")).each(function (i, update) {
            var top;

            switch (mode) {
                case "BEFORE":
                    $(update).prepend(data);
                    break;
                case "AFTER":
                    $(update).append(data);
                    break;
                case "REPLACE-WITH":
                    $(update).replaceWith(data);
                    break;
                default:
                    $(update).html(data);
                    break;
            }
        });
    }

    function asyncRequest(element, options) {
//        var confirm, loading, method, duration;
        var confirm, method;

        confirm = element.getAttribute("data-ajax-confirm");
        if (confirm && CheckConfirm) {
            CheckConfirm = false;

            SetTextMain('ConfirmMsg', confirm);
            SetValueSimpl('ConfirmFormBtn', element.getAttribute("data-ajax-Btn"));
            $('#ConfirmModal').css('display', 'flex');
            return;
        }
        else
        {
            CheckConfirm = true;

//            loading = $(element.getAttribute("data-ajax-loading"));
//            duration = parseInt(element.getAttribute("data-ajax-loading-duration"), 10) || 0;

            $.extend(options, {
                type: element.getAttribute("data-ajax-method") || undefined,
                url: element.getAttribute("data-ajax-url") || undefined,
                cache: (element.getAttribute("data-ajax-cache") || "").toLowerCase() === "true",
                beforeSend: function (xhr) {
                    var result;
                    asyncOnBeforeSend(xhr, method);
                    result = getFunction(element.getAttribute("data-ajax-begin"), ["xhr"]).apply(element, arguments);
                    if (result !== false) {
                        //     loading.show(duration);
                        xhr.form = element;   // فرم مربوط به این درخواست
                        // پاک کردن اشاره به خطاهای قبل
                        element.querySelectorAll(".error-border")
                            .forEach(e => e.classList.remove("error-border"));

                        // غیر فعال کردن دکمه سابمیت برای جلوگیری از دوبار کلیک
                        const btn = element.querySelector("button[type='submit'], input[type='submit']");

                        if (btn) {
                            xhr._btn = btn;
                            btn.disabled = true;
                            btn.classList.add("btn-loading"); // اختیاری برای استایل
                        }

                    }
                    return result;
                },
                complete: function (xhr, status) {

                    getFunction(element.getAttribute("data-ajax-complete"), ["xhr", "status"])
                        .apply(element, arguments);

                    if (xhr._btn && document.contains(xhr._btn)) {
                        xhr._btn.disabled = false;
                        xhr._btn.classList.remove("btn-loading");
                    }
                },
                success: function (data, status, xhr) {
                    asyncOnSuccess(element, data, xhr.getResponseHeader("Content-Type") || "text/html");
                    getFunction(element.getAttribute("data-ajax-success"), ["data", "status", "xhr"]).apply(element, arguments);
                },
                error: function () {
                    getFunction(element.getAttribute("data-ajax-failure"), ["xhr", "status", "error"]).apply(element, arguments);
                }
            });

            options.data.push({ name: "X-Requested-With", value: "XMLHttpRequest" });

            method = options.type.toUpperCase();
            if (!isMethodProxySafe(method)) {
                options.type = "POST";
                options.data.push({ name: "X-HTTP-Method-Override", value: method });
            }

            // change here:
            // Check for a Form POST with enctype=multipart/form-data
            // add the input file that were not previously included in the serializeArray()
            // set processData and contentType to false
            var $element = $(element);
            if ($element.is("form") && $element.attr("enctype") == "multipart/form-data") {
                var formdata = new FormData();
                $.each(options.data, function (i, v) {
                    formdata.append(v.name, v.value);
                });
                $("input[type=file]", $element).each(function () {
                    var file = this;
                    $.each(file.files, function (n, v) {
                        formdata.append(file.name, v);
                    });
                });
                $.extend(options, {
                    processData: false,
                    contentType: false,
                    data: formdata
                });
            }
            // end change

            $.ajax(options);
        }

    }


    function validate(form) {
        var validationInfo = $(form).data(data_validation);
        return !validationInfo || !validationInfo.validate || validationInfo.validate();
    }

    $(document).on("click", "a[data-ajax=true]", function (evt) {
        evt.preventDefault();
        asyncRequest(this, {
            url: this.href,
            type: "GET",
            data: []
        });
    });

    $(document).on("click", "form[data-ajax=true] input[type=image]", function (evt) {
        var name = evt.target.name,
            target = $(evt.target),
            form = $(target.parents("form")[0]),
            offset = target.offset();

        form.data(data_click, [
            { name: name + ".x", value: Math.round(evt.pageX - offset.left) },
            { name: name + ".y", value: Math.round(evt.pageY - offset.top) }
        ]);

        setTimeout(function () {
            form.removeData(data_click);
        }, 0);
    });

    $(document).on("click", "form[data-ajax=true] :submit", function (evt) {
       // ActiveButton = evt.currentTarget;
       // ActiveButton.prop("disabled", true); // Disable the button  
        var name = evt.currentTarget.name,
            target = $(evt.target),
            form = $(target.parents("form")[0]);

        form.data(data_click, name ? [{ name: name, value: evt.currentTarget.value }] : []);
        form.data(data_target, target);

        setTimeout(function () {
            form.removeData(data_click);
            form.removeData(data_target);
        }, 0);
    });

    $(document).on("submit", "form[data-ajax=true]", function (evt) {
        var clickInfo = $(this).data(data_click) || [],
            clickTarget = $(this).data(data_target),
            isCancel = clickTarget && (clickTarget.hasClass("cancel") || clickTarget.attr('formnovalidate') !== undefined);
        evt.preventDefault();
        if (!isCancel && !validate(this)) {
            return;
        }
        asyncRequest(this, {
            url: this.action,
            type: this.method || "GET",
            data: clickInfo.concat($(this).serializeArray())
        });
    });

}(jQuery));

function FindListFloorAjax(resSelect, BuildingId, Knd) {
    document.getElementById(resSelect).innerHTML = '<option value="0" class="text-danger">چند لحظه صبر کنید ...</option>';
    const xmlhttp = new XMLHttpRequest();
    xmlhttp.onload = function () {
        document.getElementById(resSelect).innerHTML = this.responseText;
    }
    xmlhttp.open("GET", "../ApiCenter?handler=ListFloors"
        + "&BuildingId=" + BuildingId.value
        + "&All=" + Knd);
    xmlhttp.send();
}
function FindListUnitAjax(resSelect, FloorId, Knd) {
    document.getElementById(resSelect).innerHTML = '<option value="0" class="text-danger">چند لحظه صبر کنید ...</option>';
    const xmlhttp = new XMLHttpRequest();
    xmlhttp.onload = function () {
        document.getElementById(resSelect).innerHTML = this.responseText;
    }
    xmlhttp.open("GET", "../ApiCenter?handler=ListUnits"
        + "&FloorId=" + FloorId.value
        + "&All=" + Knd);
    xmlhttp.send();
}




function FastReportAjax(LimitDateKnd, BtnAct, indexDate, LD) {
    const xmlhttp = new XMLHttpRequest();
    if (LD != '')
        document.getElementById(LD).value = LimitDateKnd;

    SpinnerShow('waitLimitDate' + indexDate);

    xmlhttp.onload = function () {
        const RD = JSON.parse(this.responseText);
        // ست کردن تاریخ
        document.getElementById('FromDate' + indexDate).value = RD.fromDate;
        document.getElementById('ToDate' + indexDate).value = RD.toDate;
        // اجرای گزارش
        if (BtnAct != '') {
            document.getElementById(BtnAct).value = '0';
            document.getElementById(BtnAct).click();
        }
        AreaClear('waitLimitDate' + indexDate);
    }
    xmlhttp.open("GET", "../ApiCenter?handler=LimitDate"
        + "&LimitDateKnd=" + LimitDateKnd);
    xmlhttp.send();
}
function LimitDateAjax(select, indexDate) {
    const xmlhttp = new XMLHttpRequest();
    SpinnerShow('waitLimitDate' + indexDate);

    xmlhttp.onload = function () {
        var RD = JSON.parse(this.responseText);
        // ست کردن تاریخ
        document.getElementById('FromDate' + indexDate).value = RD.fromDate;
        document.getElementById('ToDate' + indexDate).value = RD.toDate;

        AreaClear('waitLimitDate' + indexDate);
    }
    xmlhttp.open("GET", "../ApiCenter?handler=LimitDate"
        + "&LimitDateKnd=" + select.value);
    xmlhttp.send();
}

function HelpAjax(ID) {
    if (document.getElementById('HelpArea').innerHTML != "")
        return;

    const xmlhttp = new XMLHttpRequest();

    GoToPosArea("HelpArea");
    SpinnerShow('HelpArea');

    xmlhttp.onload = function () {
        document.getElementById('HelpArea').innerHTML = this.responseText;
    }
    xmlhttp.open("GET", "../ApiCenter?handler=HelpShow"
        + "&ID=" + ID);
    xmlhttp.send();
}
function CheckNewMsgAjax(ID) {
    const xmlhttp = new XMLHttpRequest();
    setInterval(function () {
        xmlhttp.onload = function () {
            if (this.responseText != "0") {
                document.getElementById('FastAlarmMsg').innerText = 'توجه: ' + this.responseText + ' پیام جدید داری';
                document.getElementById('FastAlarm').classList.remove('dis-none');
                ScrollFastAlarm();
            }
            else {
                document.getElementById('FastAlarm').classList.add('dis-none');
            }
        }
        xmlhttp.open("GET", "../ApiCenter?handler=CheckNewMsg&UserId=" + ID);
        xmlhttp.send();
    }, 60000);
}

function FindListStore(Knd, Filter, resSelect) {
    document.getElementById(resSelect).innerHTML = '<option value="0" class="text-danger">چند لحظه صبر کنید ...</option>';

    const xmlhttp = new XMLHttpRequest();

    xmlhttp.open("GET", "../ApiCenter?handler=FindListStore&Knd=" + Knd + "&Filter=" + document.getElementById(Filter).value);
    xmlhttp.send();

    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            document.getElementById(resSelect).innerHTML = this.responseText;
            $('#' + resSelect).change();
        }
    };
}

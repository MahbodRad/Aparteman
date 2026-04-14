/*console.log("site.js START");*/

var GlobalFlag = false;
var CNT = 0;
var timeLeft = 0;
var countdown;
const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))


function ToggleFullscreen() {
    if (!document.fullscreenElement &&    // alternative standard method
        !document.mozFullScreenElement && !document.webkitFullscreenElement && !document.msFullscreenElement) {  // current working methods
        if (document.documentElement.requestFullscreen) {
            document.documentElement.requestFullscreen();
        } else if (document.documentElement.msRequestFullscreen) {
            document.documentElement.msRequestFullscreen();
        } else if (document.documentElement.mozRequestFullScreen) {
            document.documentElement.mozRequestFullScreen();
        } else if (document.documentElement.webkitRequestFullscreen) {
            document.documentElement.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
        }
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        }
    }
}

function printSection(sectionId) {
    var printContent = document.getElementById(sectionId).innerHTML;
    var originalContent = document.body.innerHTML;
    document.body.innerHTML = printContent;

    //const { jsPDF } = window.jspdf;
    //const doc = new jsPDF();
    //doc.html(windows, 1, 1);
    //doc.save("sample.pdf");

    window.print();
    document.body.innerHTML = originalContent;
}
function generatePDF() {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();
    doc.html("Hello, this is a PDF generated with jsPDF!", 1, 1);
    doc.save("sample.pdf");
}
function SpinnerShow(Area, TITR) {
    //GoToPosAreaFix(Area);
    if (typeof Area != 'object') Area = '#' + Area;
    if (TITR != null) {
        $(Area).html('<div class="w-100 text-center font-20 text-primary">' + TITR + '</div><div class="w-100"><img src="../images/loading.gif" class="w-max-100 mx-auto" /><div class="timer" id="timerSpiner">0</div></div>');
    } else {
        $(Area).html('<div class="w-100"><img src="../images/loading.gif" class="w-max-100 mx-auto" /><div class="timer" id="timerSpiner">0</div></div>');
    }
    clearInterval(countdown);
    timeLeft = 0;
    countdown = setInterval(() => {
        timeLeft++;
        $('#timerSpiner').text(timeLeft);
        if (timeLeft >= 1000) {
            clearInterval(countdown);
        }
    }, 1000);
}
function SpinnerShowBtn(Btn) {if (typeof Btn != 'object')Btn = '#' + Btn;$(Btn).html('<img src="../images/loading.gif" class="w-max-40" />');}
function copyToClipboard(txt, MSG) { var $temp = $("<textarea>");$("body").append($temp);$temp.val(txt).select();document.execCommand("copy");$temp.remove();if (MSG != '') {ShowMsg('کپی در کلیبورد', MSG);}}
function copyAreaToClipboard(Area, MSG) { var copytext = EleText(Area); var input = document.createElement('textarea'); input.innerHTML = copytext; document.body.appendChild(input); input.select(); var result = document.execCommand('copy'); document.body.removeChild(input); if (MSG != '') { ShowMsg('کپی', MSG);} return result;}
function insertTextArea(textarea, text) {
    if (!textarea.value.includes(text)) {
        const position = textarea.selectionStart;
        const before = textarea.value.substring(0, position);
        const after = textarea.value.substring(position, textarea.value.length);
        textarea.value = before + text + after;
        textarea.selectionStart = textarea.selectionEnd = position + text.length;
    }
};

function isJsonString(RES) { try { if (RES == null) { SpinnerHideForm('توجه', 'عدم موفقیت در پردازش پیام'); return false; } else if (JSON.parse(RES.responseText)) return true; } catch (e) { ShowMsg('توجه', RES.responseText); return false; } }
function RowSelectColor(AREA, ROW) {SetHtml(AREA, EleHtml(AREA).replace('bg-info',''));if (typeof ROW != 'object')ROW = '#' + ROW;$(ROW).addClass("bg-info");}
function CheckErrorSelect(SEL) {if (! $('#' + SEL).html().includes('</option>')) ShowMsg('توجه', $('#' + SEL).html());}
function AreaClear(Area) { $('#' +Area).text('') }
function SpinnerShowForm(TITR) {
    $('#SpinnerMsg_TITR').text(TITR); AreaShowFlex('ModalSpiner');
    clearInterval(countdown);
    timeLeft = 0;
    countdown = setInterval(() => {
        timeLeft ++;
        $('#timerForm').text(timeLeft);
        if (timeLeft >= 1000) {
            clearInterval(countdown);
        }
    }, 1000);
}
function SpinnerHideForm(TITR = '', MSG = '') {
    AreaUp('ModalSpiner');
    clearInterval(countdown);
    if (MSG != '')
        ShowMsg(TITR, MSG);
}
function AreaShow(Area) { $('#'+Area).show(500) }
function AreaHide(Area) { $('#' + Area).hide(500) }
function AreaToggelSH(Area) { if ($('#' + Area).is(':visible')) { $('#' + Area).hide(500); } else { $('#' + Area).show(500); } }
function AreaToggel(Area) { if ($('#' + Area).is(':visible')) { $('#' + Area).slideUp(500); } else { $('#' + Area).slideDown(500); } }
function AreaUp(Area) { $('#' + Area).slideUp(500); }
function AreaDown(Area) { $('#' + Area).slideDown(500); }
function AreaDown(Area) { $('#' + Area).slideDown(500); }
function AreaShowFlex(Area) { $('#' + Area).css('display', 'flex').slideDown(500); }
function AreaHideFlex(Area) { $('#' + Area).css('display', 'none') }
function SetBackColor(Area) { $('#' +Area).addClass("bg-Learn"); }
function ShowMsgArea(Area, MSG) { $('#' +Area).html('<div class="row"><div class="col font-17"><p class="bg-Learn w-max-350 mx-auto text-center spc-pre-line"><span class="icofont-alarm font-22 text-warning mx-2"></span><span>' + MSG + '</span></p></div></div>');}
function ShowMsg(TITR, MATN) { $("#ShowMsg_TITR").text(TITR); $("#ShowMsg_MATN").html(MATN); AreaShowFlex('MsgAreaModal'); }
function ShowFastHelp(ONVAN, MATN) { ShowMsg(EleText(ONVAN), MATN); }
function ShowFrame(TITR, SRC) {
    $("#MsgForm_Frame_TITR").text(TITR); $("#MsgForm_Frame_MATN").html("<iframe src='" + SRC + "' style='width: 80%; border: 0; height: 80%;'></iframe >"); $("#MsgFrom_Frame_Btn").click() }
function MsgForm(FRM, TITR, MATN) { $('#MsgFormTITR_' + FRM).text(TITR); $('MsgFormMATN_' + FRM).html(MATN); OpenFormMsg(FRM); }
function MsgFormSpinner(FRM, TITR, MATN) { $('#MsgFormTITR_' + FRM).text(TITR); SpinnerShow('MsgFormMATN_' + FRM, MATN); OpenFormMsg(FRM); }
function SetHtml(Area, HTML) { $('#' + Area).html(HTML); }
function SelectWait(S, M) { $('#' +S).html("<option value=''>" + M + "</option>"); }
function SelectSet(S, V, T) { $('#' + S).html("<option value='" + V + "'>" + T + "</option>"); }
function SelectOptionData(S, D) { if (typeof S != 'object') S = '#' + S; return $(S).find(':selected').attr(D); }
function SelectOptionText(S) { if (typeof S != 'object') S = '#' + S; return S.options[S.selectedIndex].text; }
function MenuToggel() { if ($('#MenuArea').width() == 0) { $('#MenuArea').width(180); $('#MainArea').css("padding-right", "180px"); $('#FooterArea').css("padding-right", "180px"); $('footer-page').width($('footer-page').width() - 180); } else { $('#MenuArea').width(0); $('#MainArea').css("padding-right", "0px"); $('#FooterArea').css("padding-right", "0px"); $('footer-page').css("width", "100%"); }; };

function GoToPos(P) { $('body,html').animate({ scrollTop: P }, 800); return false; }
function GoToPosArea(Area) { var x = $('#' + Area).position(); GoToPos(x.top - 50); }
function GoToPosAreaFix(Area) {
    if (typeof Area != 'object') Area = '#' + Area;
    document.querySelector(Area).scrollIntoView({ behavior: 'smooth' });
}
function GoToPosTopArea(Area) { var x = $('#' + Area).position(); GoToPos(x.top - 100); }
function GoToPosChart(Area) { var x = $('#' + Area).position(); GoToPos(x.top ); }
function Zoom(Pic) { SetHtml('ZoomPic', EleHtml(Pic)); $("#Zoom_Btn").click(); }
LoginFastComp = function (RES)
{
    if (RES.responseText.toUpperCase() == "OK")
        ShowMsgArea('RESLoginFastMSG', 'ورود شما به سیستم ثبت شد. دوباره گزارش بگیرید');
    else
    {
        ShowMsgArea('RESLoginFast', RES.responseText);
    }
}
LoginComp = function (RES) {
    SpinnerHideForm();
    let RJS = RES.responseJSON;
    if (RJS.success.toUpperCase() == "OK") {
        window.location = RJS.gopage;
    }
    else {
        SetTextMain('RES', RJS.message);
        ShowMsg('خطا در ورود به سیستم', RJS.message);
    }
}
RegisterComplete = function (RES) {
    SpinnerHideForm();
    let JS = RES.responseJSON;
    if (JS.success.toUpperCase() != "OK") {
        ShowMsg('خطا در ثبت نام', JS.message);
        ShowMsgArea('resultMessage', JS.message);
    }
    else
    {
        SetValue('userNameLogin','userNameRegister');
        AreaClear('RegisterArea');
        AreaShow('LoginArea');
    }
}

function ScrollSlave(Master, Slave) { $('#' + Slave).scrollLeft($(Master).scrollLeft()); }
function ScrollLeft(Element, Scale = '200') {
    $('#' + Element).animate({ scrollLeft: '-=' + Scale }, 1000);
}
function ScrollRight(Element, Scale = '200') {
    $('#' + Element).animate({ scrollLeft: '+=' + Scale }, 1000);
}
function ScrollFastAlarm() {
    var Scale = $('#menu-page-btn').position()
    $('#menu-page-btn').animate({ scrollLeft: '=' + Scale.left }, 1000);
}
function AreaRemove(A) {
    $('#' + A).remove();
}
function CheckBoxClick(CHK) {
    // کلید روی یک چک باکس
    if ($(CHK).val() == 'YES') {
        $(CHK).val('NO');
    }
    else {
        $(CHK).val('YES');
    }
}
function ControlPersianDate(DD) {
    if (DD == '')
        $(".usage").persianDatepicker({ theme: "latoja", formatDate: "YYYY/0M/0D", cellWidth: 30, cellHeight: 26, fontSize: 15, });
    else
        $("#"+DD).persianDatepicker({ theme: "latoja", formatDate: "YYYY/0M/0D", cellWidth: 30, cellHeight: 26, fontSize: 15, });
}

function SetLocalValue(V) { /*پرکردن مقدار جاری متغیر روی فرم*/ $('#LocalValue').val(V);}
function ChangIcon(S, I1, I2) {
    // تغییر یک آیکون
    if (typeof S != 'object')
        S = '#' + S;

    Rotate(S, 'Z');
    if ($(S).hasClass(I1)) {
        $(S).removeClass(I1);
        $(S).addClass(I2);
    }
    else {
        $(S).removeClass(I2);
        $(S).addClass(I1);
    }
}
function ChangClass(S, A, R) {
    // تغییر یک آیکون
    if (typeof S != 'object')
        S = '#' + S;

    $(S).removeClass(R);
    $(S).addClass(A);
}
function ChangValue(V, A, B) {
    // تغییر مقدار یک و دو
    if (EleValue(V) == A)
        SetValueSimplMain(V, B);
    else
        SetValueSimplMain(V, A);
}
function Rotate(ELM, XYZ) {
    if (XYZ == 'Y')
        $(ELM).addClass('bg-info');

    if ($(ELM).hasClass('rotated360' + XYZ)) {
        $(ELM).removeClass('rotated360' + XYZ);
        $(ELM).addClass('rotated0');
    }
    else {
        $(ELM).removeClass('rotated0');
        $(ELM).addClass('rotated360' + XYZ);
    }
}

function MenuItemClick(ELM) {
    $('#waitbody').css('display', 'flex');
    $(ELM).addClass('menu-Item');
    $(ELM).css('width', '120px');
    clearInterval(countdown);
    timeLeft = 0;
    countdown = setInterval(() => {
        timeLeft++;
        $('#timerWait').text(timeLeft);
        if (timeLeft >= 1000) {
            clearInterval(countdown);
        }
    }, 1000);

}

JsonResult = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES))
    {
        const frm = RES.form
        const JS = RES.responseJSON;
        if (JS.success.toUpperCase() == "OK")
            SpinnerHideForm('انجام دستور', JS.message);
        else
        {
            if (JS.field)
            {
                const el = frm.querySelector('[name="' + JS.field + '"]');
                if (el)
                {
                    el.focus();
                    el.classList.add("error-border");
                }
            }
            SpinnerHideForm('خطا در انجام دستور', JS.message);
        };
    }
}

JsonResultOK = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES))
    {
        const JS = RES.responseJSON;
        if (JS.success.toUpperCase() != "OK")
            ShowMsg('انجام دستور', JS.message);
    }
}

JsonClose_A = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES)) {
        const frm = RES.form
        const JS = RES.responseJSON;
        if (JS.success.toUpperCase() == "OK")
            CloseFormMsg('A');
        else {
            if (JS.field) {
                const el = frm.querySelector('[name="' + JS.field + '"]');
                if (el) {
                    el.focus();
                    el.classList.add("error-border");
                }
            }
        };
        ShowMsg('انجام دستور', JS.message);
    }
}
JsonClose_B = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES)) {
        const frm = RES.form
        const JS = RES.responseJSON;
        if (JS.success.toUpperCase() == "OK")
            CloseFormMsg('B');
        else {
            if (JS.field) {
                const el = frm.querySelector('[name="' + JS.field + '"]');
                if (el) {
                    el.focus();
                    el.classList.add("error-border");
                }
            }
        };
        ShowMsg('انجام دستور', JS.message);
    }
}
JsonClose_BA = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES)) {
        const frm = RES.form
        const JS = RES.responseJSON;
        if (JS.success.toUpperCase() == "OK") {
            CloseFormMsg('B');
            CloseFormMsg('A');
        }
        else {
            if (JS.field) {
                const el = frm.querySelector('[name="' + JS.field + '"]');
                if (el) {
                    el.focus();
                    el.classList.add("error-border");
                }
            }
        };
        ShowMsg('انجام دستور', JS.message);
    }
}
JsonClose_C = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES)) {
        const frm = RES.form
        const JS = RES.responseJSON;
        if (JS.success.toUpperCase() == "OK")
            CloseFormMsg('C');
        else {
            if (JS.field) {
                const el = frm.querySelector('[name="' + JS.field + '"]');
                if (el) {
                    el.focus();
                    el.classList.add("error-border");
                }
            }
        };
        ShowMsg('انجام دستور', JS.message);
    }
}
JsonClose_CB = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES)) {
        const frm = RES.form
        const JS = RES.responseJSON;
        if (JS.success.toUpperCase() == "OK") {
            CloseFormMsg('C');
            CloseFormMsg('B');
        }
        else {
            if (JS.field) {
                const el = frm.querySelector('[name="' + JS.field + '"]');
                if (el) {
                    el.focus();
                    el.classList.add("error-border");
                }
            }
        };
        ShowMsg('انجام دستور', JS.message);
    }
}
JsonClose_CBA = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES)) {
        const frm = RES.form
        const JS = RES.responseJSON;
        if (JS.success.toUpperCase() == "OK") {
            CloseFormMsg('C');
            CloseFormMsg('B');
            CloseFormMsg('A');
        }
        else {
            if (JS.field) {
                const el = frm.querySelector('[name="' + JS.field + '"]');
                if (el) {
                    el.focus();
                    el.classList.add("error-border");
                }
            }
        };
        ShowMsg('انجام دستور', JS.message);
    }
}

function CloseFormMsg(FRM)
{
    AreaHide('ModalForm_' + FRM);
    AreaClear('MsgFormMATN_' + FRM);
}

function OpenFormMsg(FRM)
{
    AreaShowFlex('ModalForm_' + FRM);
}


//$(document).ready(function () {
//    $(window).scroll(function () {
//        if ($(this).scrollTop() > 50) {
//            $('#BtnGoToTop').fadeIn();
//        } else {
//            $('#BtnGoToTop').fadeOut();
//        }
//  //      if (($(this).scrollTop() - document.getElementById("ListAsnad_MS").offsetTop) > 50) {
//  //          alert('OK');
//  //      }
//    });

//    // scroll body to 0px on click
//    $('#BtnGoToTop').click(function () {
//        $('body,html').animate({
//            scrollTop: 0
//        }, 800);
//        return false;
//    });
//});
function GoToTop() {
    $('body,html').animate({
        scrollTop: 0
    }, 800);
    return false;
}
function PikerDate(A) {
    if (A=='')
        $(".usage").persianDatepicker({ theme: "latoja", formatDate: "YYYY/0M/0D", cellWidth: 30, cellHeight: 26, fontSize: 15, });
    else
        $("#"+A).persianDatepicker({ theme: "latoja", formatDate: "YYYY/0M/0D", cellWidth: 30, cellHeight: 26, fontSize: 15, });
}
function AddSelectedItem(E, ID) {
    // کنترل میکند که آیا مقدار داده شده در مقادیر ذخیره شده است یا نه
    // اگر نبود اضافه شود و اگر بود حذف شود
    var IDValue = ',' + ID + ',';
    var IdValue = EleValue(E);
    if (IdValue.includes(IDValue) == true)
        IdValue = IdValue.replace(IDValue, ',');
    else
        IdValue += ID + ',';
    SetValueSimplMain(E, IdValue);
}
function EnterKey(e, BTN) {
    //فشردن کلید اینتر برای کلیک روی یک دکمه دیگر
    var code = e.key;
    if (code === "Enter") {
        ClickBtnSimpl(BTN, '');
    }
}

function EscKey(e, BTN) {
    //فشردن کلید Scape برای کلیک روی یک دکمه دیگر
    var code = e.key;
    if (code === "Escape") {
        ClickBtnSimpl(BTN, '');
    }
}
function EnglishNumber(NUMBER) {
    // تبدیل اعداد به حروف انگلیسی
    if (NUMBER=="")
        return NUMBER
    else
        return NUMBER.replace("۰", "0").replace("۱", "1").replace("۲", "2").replace("۳", "3").replace("۴", "4").replace("۵", "5").replace("۶", "6").replace("۷", "7").replace("۸", "8").replace("۹", "9");
}

function onlyNumber(INP) {
    // فقط عدد بتونه بنویسه
    NUM = EnglishNumber(INP.value);
    INP.value = NUM.replace(/[^0-9\,.,+-]/, '');
}
function AddComma(INP) {
    // اضافه کردن کاما به اعداد
    NUM = EnglishNumber(INP.value);

    INP.value = numberWithCommas(NUM.replace(/[^0-9\,.,-]/, ''));
}
function numberWithCommas(x) {
    //افزودن کاما
    x = x.toString();
    x = numberClearCommas(x);
    if (x != '0')
        x = x.replace(/^0+/, '');

    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function numberClearCommas(x) {
    // حذف کاماها از متن
    x = x.toString();
    return x.replace(/,/g, "");
}
function CountChar(T, C) {
    // تعداد یک کاراکتر در متن
    let count = 0;
    for (const char of T) {
        if (char === C) {
            count++;
        }
    }
    return count;
}

function AddDash(INP) {
    //اضافه کردن خط تیره به متن
    $(INP).val(numberWithDash($(INP).val()));
}
function numberWithDash(x) {
    x = x.toString();
    x = numberClearDash(x);

    N = '';
    j = 0;
    i = x.length;
    L = 0;
    while (i > 0) {
        if (j == 4) {
            N += '_';
            j = 0;
        }
        j += 1;
        N += x.substring(L, L+1);
        i -= 1;
        L += 1;
    }

    return N.toString();
}

function numberClearDash(x) {
    x = x.toString();
    return x.replace(/_/g, "");
}
function AddNumber(INP, N) {
    // افزودن یک مقدار شمارشی به یک عدد
    const ELE = '#' + INP;
    let NUMBER = $(ELE).val();
    NUMBER = parseInt(NUMBER) + N;
    $(ELE).val(NUMBER);
}
function AddNumbertext(INP, N) {
    // افزودن یک مقدار شمارشی به یک عدد
    const ELE = '#' + INP;
    let NUMBER = $(ELE).text();
    NUMBER = parseInt(NUMBER) + N;
    $(ELE).text(NUMBER);
}


function EleValue(X) {
    if (typeof X != 'object')
        X = '#' + X;
    return $(X).val();
}
function EleText(X) {
    if (typeof X != 'object')
        X = '#' + X;
    return $(X).text();
}
function EleHtml(X) {
    if (typeof X != 'object')
        X = '#' + X;
    return $(X).html();
}
function RowSelect(ROW) {
    if (typeof ROW != 'object')
        ROW = '#' + ROW;

    if ($(ROW).hasClass("Row_Select"))
        $(ROW).removeClass("Row_Select");
    else
        $(ROW).addClass("Row_Select");
}

function Btn_Select(Act, BTNArea) {
    //در ناحیه مشخص شده عنصرهای مختلف را پیدا میکند و رنگشون رااز حالت انتخاب در میاره و اونی که فعال است را رنگش را به حالات انتخاب میبرد
    $('#' + BTNArea).find('button').removeClass("btn_Selected");
    if (typeof Act != 'object')
        Act = '#' + Act;
    $(Act).addClass("btn_Selected");

}
function Btn_SelectWarning(Act, BTNArea) {
    //در ناحیه مشخص شده عنصرهای مختلف را پیدا میکند و رنگشون رااز حالت انتخاب در میاره و اونی که فعال است را رنگش را به حالات انتخاب میبرد
    $('#' + BTNArea).find('button').removeClass("btn_SelectWarning");
    if (typeof Act != 'object')
        Act = '#' + Act;
    $(Act).addClass("btn_SelectWarning");
}
function Btn_SelectFlat(BTNAct, BTNArea) {
    //در ناحیه مشخص شده عنصرهای مختلف را پیدا میکند و رنگشون رااز حالت انتخاب در میاره و اونی که فعال است را رنگش را به حالات انتخاب میبرد
    $('#' + BTNArea).find('button').removeClass("btn_SelectedFlat");
    if (typeof BTNAct != 'object')
        BTNAct = '#' + BTNAct;
    const h = BTNAct.height;
    BTNAct.height = 0;
    $(BTNAct).addClass("btn_SelectedFlat");


    if ($(BTNAct).hasClass('rotated360X')) {
        $(BTNAct).removeClass('rotated360X');
        $(BTNAct).addClass('rotated0X');
    }
    else {
        $(BTNAct).removeClass('rotated0X');
        $(BTNAct).addClass('rotated360X');
    }
}
function Area_Select(Act, Area, KND) {
    //در ناحیه مشخص شده عنصرهای مختلف را پیدا میکند و پنهانشون میکنه و فقط یکی را نمایش میده
    $('#'+Area).find(KND).slideUp();
    if (typeof Act != 'object')
        Act = '#' + Act;
    $(Act).slideDown(500);
}
// کنترل اینکه بعد از گزارش چاپ میگیرد یا نه
function ControlChap(BTN) {
    SpinnerHideForm();

    var knd = EleValue(BTN);
    if (knd == "0" || (knd == ""))
        return;
     
    if (knd == "CHAP") {
        ChapShow();
        return;
    }

    if (knd == "PDF") {
        PdfShow();
        return;
    }

    if (knd == "XLS")
        $('#Excel_Btn').click();

    if (knd == "SND")
        ShowMsg('ارسال پیام', 'گزارش ارسال شد');

}

ShowChart = function (RES) {
    if (isJsonString(RES.responseText)) {

        var _data = RES.responseJSON;
        var _chartArea = _data[0];
        var _chartType = _data[1];
        var _chartLabels = _data[2];
        var _charData = _data[3];
        var Tp = _data[4];
        SetHtml(_chartArea, '<canvas class="w-max-1000 h-max-700 mx-auto" id="'+_chartArea + '_Chart"></canvas>')
        var ctx = document.getElementById(_chartArea +"_Chart").getContext('2d');
        new Chart(ctx, {
            type: _chartType,
            data: {
                labels: _chartLabels,
                datasets: [
                    {
                        label: "",
                        data: _charData,
                        backgroundColor: [
                            'rgba(255, 0, 70, 0.2)',
                            'rgba(245, 10, 140, 0.2)',
                            'rgba(235, 20, 210, 0.2)',

                            'rgba(225, 30, 70, 0.2)',
                            'rgba(215, 40, 140, 0.2)',
                            'rgba(205, 50, 210, 0.2)',

                            'rgba(195, 60, 70, 0.2)',
                            'rgba(185, 70, 140, 0.2)',
                            'rgba(175, 80, 210, 0.2)',

                            'rgba(165, 90, 70, 0.2)',
                            'rgba(155, 100, 140, 0.2)',
                            'rgba(145, 110, 210, 0.2)',

                            'rgba(135, 120, 70, 0.2)',
                            'rgba(125, 130, 140, 0.2)',
                            'rgba(115, 140, 210, 0.2)',

                            'rgba(105, 150, 70, 0.2)',
                            'rgba(95, 160, 140, 0.2)',
                            'rgba(85, 170, 210, 0.2)',

                            'rgba(75, 180, 70, 0.2)',
                            'rgba(65, 190, 140, 0.2)',
                            'rgba(55, 200, 210, 0.2)',

                            'rgba(45, 210, 70, 0.2)',
                            'rgba(35, 220, 140, 0.2)',
                            'rgba(25, 230, 210, 0.2)',

                            'rgba(15, 240, 70, 0.2)',
                            'rgba(5, 250, 140, 0.2)',
                            'rgba(0, 255, 210, 0.2)',
                        ],
                        borderColor: "brown",
                        borderWidth: 1,
                    }
                ],
            }
            , options: {
                responsive: true,
                legend: {
                    title: {
                        bodyFontFamily: "'Tahoma'",
                    },
                    labels: {
                        fontFamily: "'Tahoma'",
                    },
                },
                scales: {
                    y: {
                        // reverse: true,
                        type: 'linear',
                        display: true,
                        position: 'right',
                    },
                },
                //plugins: {
                //    datalabels: {
                //        display: true,
                //        align: 'end',
                //        anchor: 'end',
                //        formatter: (value) => {
                //            return value; // Display the value on top of the columns  
                //        }
                //    }
                //}  
            }
        });

        var datavalu = "";
        var index = -1;
        var rj = 0;
        _chartLabels.forEach(function (item) {
            rj++;
            index++;
            datavalu = datavalu + '<div class="border-bottom border-secondary  p-1 d-flex justify-content-between"><span><span class="text-danger">' + rj + ' -> </span>' + item + '</span><span>' + _charData[index] +'</span> </div>'
            
        });
        SetHtml(_chartArea+'_Data', datavalu);
    }
}

ShowChart2 = function (RES) {
    if (isJsonString(RES.responseText)) {

        var _data = RES.responseJSON;
        var _chartArea = _data[0];
        var _chartType = _data[1];
        var _chartLabels = _data[2];
        var _charLabelA = _data[3];
        var _charDataA = _data[4];
        var _charLabelB = _data[5];
        var _charDataB = _data[6];
        var Tp = _data[7];
        SetHtml(_chartArea, '<canvas class="w-max-1000 h-max-700 mx-auto" id="' + _chartArea + '_Chart"></canvas>')
        var ctx = document.getElementById(_chartArea + "_Chart").getContext('2d');
        new Chart(ctx, {
            type: _chartType,
            data: {
                labels: _chartLabels,
                datasets: [
                    {
                        label: _charLabelA,
                        data: _charDataA,
                        backgroundColor: [
                            'rgba(255, 0, 70, 0.2)',
                            'rgba(245, 10, 140, 0.2)',
                            'rgba(235, 20, 210, 0.2)',

                            'rgba(225, 30, 70, 0.2)',
                            'rgba(215, 40, 140, 0.2)',
                            'rgba(205, 50, 210, 0.2)',

                            'rgba(195, 60, 70, 0.2)',
                            'rgba(185, 70, 140, 0.2)',
                            'rgba(175, 80, 210, 0.2)',

                            'rgba(165, 90, 70, 0.2)',
                            'rgba(155, 100, 140, 0.2)',
                            'rgba(145, 110, 210, 0.2)',

                            'rgba(135, 120, 70, 0.2)',
                            'rgba(125, 130, 140, 0.2)',
                            'rgba(115, 140, 210, 0.2)',

                            'rgba(105, 150, 70, 0.2)',
                            'rgba(95, 160, 140, 0.2)',
                            'rgba(85, 170, 210, 0.2)',

                            'rgba(75, 180, 70, 0.2)',
                            'rgba(65, 190, 140, 0.2)',
                            'rgba(55, 200, 210, 0.2)',

                            'rgba(45, 210, 70, 0.2)',
                            'rgba(35, 220, 140, 0.2)',
                            'rgba(25, 230, 210, 0.2)',

                            'rgba(15, 240, 70, 0.2)',
                            'rgba(5, 250, 140, 0.2)',
                            'rgba(0, 255, 210, 0.2)',
                        ],
                        borderColor: "brown",
                        borderWidth: 1,
                    },
                    {
                        label: _charLabelB,
                        data: _charDataB,
                        backgroundColor: [
                            'rgba(255, 0, 70, 0.2)',
                            'rgba(245, 10, 140, 0.2)',
                            'rgba(235, 20, 210, 0.2)',

                            'rgba(225, 30, 70, 0.2)',
                            'rgba(215, 40, 140, 0.2)',
                            'rgba(205, 50, 210, 0.2)',

                            'rgba(195, 60, 70, 0.2)',
                            'rgba(185, 70, 140, 0.2)',
                            'rgba(175, 80, 210, 0.2)',

                            'rgba(165, 90, 70, 0.2)',
                            'rgba(155, 100, 140, 0.2)',
                            'rgba(145, 110, 210, 0.2)',

                            'rgba(135, 120, 70, 0.2)',
                            'rgba(125, 130, 140, 0.2)',
                            'rgba(115, 140, 210, 0.2)',

                            'rgba(105, 150, 70, 0.2)',
                            'rgba(95, 160, 140, 0.2)',
                            'rgba(85, 170, 210, 0.2)',

                            'rgba(75, 180, 70, 0.2)',
                            'rgba(65, 190, 140, 0.2)',
                            'rgba(55, 200, 210, 0.2)',

                            'rgba(45, 210, 70, 0.2)',
                            'rgba(35, 220, 140, 0.2)',
                            'rgba(25, 230, 210, 0.2)',

                            'rgba(15, 240, 70, 0.2)',
                            'rgba(5, 250, 140, 0.2)',
                            'rgba(0, 255, 210, 0.2)',
                        ],
                        borderColor: "darkblue",
                        borderWidth: 1,
                    },

                ],
            }
            , options: {
                responsive: true,
                legend: {
                    title: {
                        bodyFontFamily: "'Tahoma'",
                    },
                    labels: {
                        fontFamily: "'Tahoma'",
                    },
                },
                scales: {
                    y: {
                        // reverse: true,
                        type: 'linear',
                        display: true,
                        position: 'right',
                    },
                },
            }
        });
    }
}

function ChapShow() {
    // تابع نمایش PDF برای چاپ گرفتن 
    try {
        printJS('../Uploads/' + EleValue('ChapName') + ".pdf?" + $.now())
    }
    catch {
        PdfShow();
    }
}
function ExcelShow() {
    $("#ChapLink").attr("href", "../Uploads/" + EleValue('ChapName') + ".xlsx?" + $.now());
    document.getElementById("ChapLink").click();
}
function ExcelWebShow() {
    $("#ChapLink").attr("href", "../Uploads/" + EleValue('ChapName') + ".html?" + $.now());
    document.getElementById("ChapLink").click();
}
function PdfShow() {
    $("#ChapLink").attr("href", "../Uploads/" + EleValue('ChapName') + ".pdf?" + $.now());
    document.getElementById("ChapLink").click();
}
function expand(obj) {
    obj.size = 5;
}
function unexpand(obj) {
    obj.size = 1;
}
function SetTextMain(A = '', T) {
    if (A == '')
        return;
    if (typeof A != 'object')
        A = '#' + A;
    $(A).text(T);
}
function SetSrc(Elm, Vlu) {
    $('#' + Elm).attr('src', Vlu);
}
function GetSrc(ELM) {
    const SRC = $(ELM).attr('src');
    return SRC;
}

function SetText(A1, T1, A2, T2, A3, T3, A4, T4, A5, T5, A6, T6, A7, T7, A8, T8, A9, T9, A10, T10) {
    //نوشتن متن برای 10 ناحیه
    SetTextMain(A1, T1);
    SetTextMain(A2, T2);
    SetTextMain(A3, T3);
    SetTextMain(A4, T4);
    SetTextMain(A5, T5);
    SetTextMain(A6, T6);
    SetTextMain(A7, T7);
    SetTextMain(A8, T8);
    SetTextMain(A9, T9);
    SetTextMain(A10, T10);
}
function SetValueMain(E = '', V) {
    if (E == '')
        return;
    if (typeof E != 'object')
        E = '#' + E;
     $(E).val(EleValue(V));
}
function SetValue(E1 , V1, E2 , V2, E3 , V3, E4 , V4, E5 , V5, E6 , V6, E7 , V7, E8 , V8, E9 , V9, E10 , V10) {
//جایگزینی مقدار برای 10 عنصر
    SetValueMain(E1, V1);
    SetValueMain(E2, V2);
    SetValueMain(E3, V3);
    SetValueMain(E4, V4);
    SetValueMain(E5, V5);
    SetValueMain(E6, V6);
    SetValueMain(E7, V7);
    SetValueMain(E8, V8);
    SetValueMain(E9, V9);
    SetValueMain(E10, V10);
}
function SetValueSimplMain(F = '', V = '') {
    if (F == '')
        return;
    if (typeof F != 'object')
        F = '#' + F;
    $(F).val(V);
}
function SetValueSimpl(F1, V1, F2, V2, F3, V3, F4, V4, F5, V5, F6, V6, F7, V7, F8, V8, F9, V9, F10, V10) {
    // مقدار دهی ساده برای 10 فیلد
    SetValueSimplMain(F1, V1);
    SetValueSimplMain(F2, V2);
    SetValueSimplMain(F3, V3);
    SetValueSimplMain(F4, V4);
    SetValueSimplMain(F5, V5);
    SetValueSimplMain(F6, V6);
    SetValueSimplMain(F7, V7);
    SetValueSimplMain(F8, V8);
    SetValueSimplMain(F9, V9);
    SetValueSimplMain(F10, V10);
}
function ClickBtnSimpl(BTN, VALU, F1, V1, F2, V2, F3, V3, F4, V4, F5, V5, F6, V6, F7, V7, F8, V8, F9, V9, F10, V10) {
// مقدار دکمه و سایر فیلدها را به صورت ساده مقدار گزاری کند و کلیک کند
    SetValueSimpl(F1, V1, F2, V2, F3, V3, F4, V4, F5, V5, F6, V6, F7, V7, F8, V8, F9, V9, F10, V10);

    if (typeof BTN != 'object')
        BTN = '#' + BTN;
    if (VALU != '')
        $(BTN).val(VALU);

    $(BTN).click();
}
function ClickBtnValue(BTN, Element, F1, EL1, F2, EL2, F3, EL3, F4, EL4, F5, EL5, F6, EL6, F7, EL7, F8, EL8, F9, EL9, F10, EL10) {
// مقدار دکمه و سایر فیلدها را از مقدار سایر عنصرها مقدارگزاری کند و کلیک کند
    SetValue(F1, EL1, F2, EL2, F3, EL3, F4, EL4, F5, EL5, F6, EL6, F7, EL7, F8, EL8, F9, EL9, F10, EL10);

    if (typeof BTN != 'object')
        BTN = '#' + BTN;
    if (Element != '')
        $(BTN).val(EleValue(Element));

    $(BTN).click();
}
function ClickBtnSimplValue(BTN, VALU, F1 = '', EL1, F2 = '', EL2, F3 = '', EL3, F4 = '', EL4, F5 = '', EL5, F6 = '', EL6, F7 = '', EL7, F8 = '', EL8, F9 = '', EL9, F10 = '', EL10) {
    // مقدار یک دکمه را به صورت ساده بنویسید
    // مقادیر 10 عنصرهای دیگر را از عنصرهای دیگر بگیرد 
    SetValue(F1, EL1, F2, EL2, F3, EL3, F4, EL4, F5, EL5, F6, EL6, F7, EL7, F8, EL8, F9, EL9, F10, EL10);

    if (typeof BTN != 'object')
        BTN = '#' + BTN;
    if (VALU != '')
        $(BTN).val(VALU);

    $(BTN).click();

}
function ClickBtnValueSimpl(BTN, Element, F1, V1, F2, V2, F3, V3, F4, V4, F5, V5, F6, V6, F7, V7, F8, V8, F9, V9, F10, V10) {
    // مقدار دکمه و سایر فیلدها را به صورت ساده مقدار گزاری کند و کلیک کند
    SetValueSimpl(F1, V1, F2, V2, F3, V3, F4, V4, F5, V5, F6, V6, F7, V7, F8, V8, F9, V9, F10, V10);

    if (typeof BTN != 'object')
        BTN = '#' + BTN;
    if (Element != '')
        $(BTN).val(EleValue(Element));

    $(BTN).click();
}
function ClickBtn(BTN) {
    document.getElementById(BTN).click();
//    if (typeof BTN != 'object')
//        BTN = '#' + BTN;
//    $(BTN).click();
}
// اجرای گزارش گرفتن سریع
function FastReportAjax(DATE, BTN, FORM, LD) {
// ابتدا تاریخ ست میشود و سپس گزارش گرفته میشود
    ClickBtnSimpl('LimitDateBtn', BTN
        , 'LimitDateKnd', DATE
        , 'LimitDateForm', FORM
        , BTN, '0');
    if (LD != '') {
        SetValueSimpl(LD, DATE);
    }
}
//-- توابع انتخاب تاریخ
function LimitDateAjax(Lim,FORM) {
    // فقط تاریخ ست میشود و گزارش گرفته نمیشود
    SetValueSimpl('LimitDateBtn', '');
    ClickBtnSimpl('LimitDateBtn', ''
        , 'LimitDateKnd', EleValue(Lim)
        , 'LimitDateForm', FORM);
    SpinnerShow('waitLimitDate' + FORM);
}
// بعد از گرفتن تاریخ دکمه گزارش کلیک شود
LimitDateCompelete = function (result) {
    // تاریخ
    if (isJsonString(result.responseText)) {
        var RD = JSON.parse(result.responseText);
        var FORM = EleValue('LimitDateForm');
        // دکمه گزارش گیری
        var BTN = EleValue('LimitDateBtn');
        // ست کردن تاریخ
        SetValueSimpl('FromDate' + FORM, RD.fromDate
            , 'ToDate' + FORM, RD.toDate);
        AreaClear('waitLimitDate' + FORM);
        // اجرای گزارش
        if (BTN != '') {
            $('#' + BTN).click();
        }
    }
}

function GetPhoto(FILE, NAME, PHOTO) {
    var file = FILE.files[0];
    var filename = file.name;
    if (NAME != "") {
        SetText(NAME, filename);
    }
    if (FILE.files && FILE.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#' + PHOTO).attr('src', e.target.result);
        };
        reader.readAsDataURL(FILE.files[0]);
    }
}

function GetFilePhoto(FILE, NAME, KND, PHOTO) {
    var file = FILE.files[0];
    var filename = file.name;
    if (FILE.files && FILE.files[0]) {
        SetText(NAME, filename);
        KND = '#' + KND;
        $(KND).removeClass('icofont-upload');
        $(KND).removeClass('icofont-image');
        $(KND).removeClass('icofont-file-text');
        $(KND).removeClass('icofont-file-bmp');
        $(KND).removeClass('icofont-file-word');
        $(KND).removeClass('icofont-file-pdf');
        $(KND).removeClass('icofont-file-gif');
        $(KND).removeClass('icofont-file-excel');
        $(KND).removeClass('icofont-exclamation');
        $('#' + PHOTO).attr('src', '/Uploads/Blog/0.jpg');
        $(KND).addClass('dis-none');
        $('#' + PHOTO).addClass('dis-none');

        var extension = filename.substr((filename.lastIndexOf('.') + 1));
        extension = extension.toUpperCase();
        switch (extension) {
            case 'JPG':
            case 'JPEG':
                $(KND).addClass('icofont-image');
                break;
            case 'PNG':
                $(KND).addClass('icofont-file-gif');
                break;
            case 'BMP':
                $(KND).addClass('icofont-file-bmp');
                break;
            case 'TXT':
                $(KND).addClass('icofont-file-text');
                break;
            case 'XLS':
            case 'XLSX':
                $(KND).addClass('icofont-file-excel');
                break;
            case 'DOC':
            case 'DOCX':
                $(KND).addClass('icofont-file-word');
                break;
            case 'PDF':
                $(KND).addClass('icofont-file-pdf');
                break;
            case 'ZIP':
            case 'RAR':
                $(KND).addClass('icofont-file-zip');
                break;
            default:
                $(KND).addClass('icofont-exclamation');
        }

        if (extension  == 'JPG' || extension == 'JPEG' || extension == 'PNG' || extension == 'BMP') {
            if (FILE.files && FILE.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    $('#' + PHOTO).attr('src', e.target.result);
                };
                reader.readAsDataURL(FILE.files[0]);
            }
            $('#' + PHOTO).removeClass('dis-none');
        }
        else
            $(KND).removeClass('dis-none');

    }
}

function GetFile(FILE, NAME, KND) {
    var file = FILE.files[0];
    var filename = file.name;
    if (FILE.files && FILE.files[0]) {
        SetText(NAME, filename);
        KND = '#' + KND;
        $(KND).removeClass('icofont-upload');
        $(KND).removeClass('icofont-image');
        $(KND).removeClass('icofont-file-text');
        $(KND).removeClass('icofont-file-bmp');
        $(KND).removeClass('icofont-file-word');
        $(KND).removeClass('icofont-file-pdf');
        $(KND).removeClass('icofont-file-gif');
        $(KND).removeClass('icofont-file-excel');
        $(KND).removeClass('icofont-exclamation');
        var extension = filename.substr((filename.lastIndexOf('.') + 1));
        extension = extension.toUpperCase();
        switch (extension) {
            case 'JPG':
            case 'JPEG':
                $(KND).addClass('icofont-image');
                break;
            case 'PNG':
                $(KND).addClass('icofont-file-gif');
                break;
            case 'BMP':
                $(KND).addClass('icofont-file-bmp');
                break;
            case 'TXT':
                $(KND).addClass('icofont-file-text');
                break;
            case 'XLS':
            case 'XLSX':
                $(KND).addClass('icofont-file-excel');
                break;
            case 'DOC':
            case 'DOCX':
                $(KND).addClass('icofont-file-word');
                break;
            case 'PDF':
                $(KND).addClass('icofont-file-pdf');
                break;
            case 'ZIP':
            case 'RAR':
                $(KND).addClass('icofont-file-zip');
                break;
            default:
                $(KND).addClass('icofont-exclamation');
        }
    }
}

function FindListGrp(Active, BTN, resSelect) {
    // تغییر رنگ دگمه با انتخاب
    if ($(Active).hasClass('btn_SelectedFlat')) {
        $(Active).removeClass('btn_SelectedFlat');
    }
    else {
        $(Active).addClass('btn_SelectedFlat');
    }
    // ساخت مقدار دسته های انتخاب شده

    var GrpKnd = '';
    if ($('#' + BTN + 'A').hasClass('btn_SelectedFlat')) { GrpKnd += 'A' }
    if ($('#' + BTN + 'B').hasClass('btn_SelectedFlat')) { GrpKnd += 'B' }
    if ($('#' + BTN + 'C').hasClass('btn_SelectedFlat')) { GrpKnd += 'C' }
    // کلیک برروی جستجوی لیست گروهها
    FindListGrpAjax(GrpKnd, resSelect);
}

function FindListGrpShop(Active, BTN, resSelect) {
    // تغییر رنگ دگمه با انتخاب
    if ($(Active).hasClass('btn_SelectedFlat')) {
        $(Active).removeClass('btn_SelectedFlat');
    }
    else {
        $(Active).addClass('btn_SelectedFlat');
    }
    // ساخت مقدار دسته های انتخاب شده

    var GrpKnd = '';
    if ($('#' + BTN + 'A').hasClass('btn_SelectedFlat')) { GrpKnd += 'A' }
    if ($('#' + BTN + 'B').hasClass('btn_SelectedFlat')) { GrpKnd += 'B' }
//    if ($('#' + BTN + 'C').hasClass('btn_SelectedFlat')) { GrpKnd += 'C' }
    // کلیک برروی جستجوی لیست گروهها
    FindListGrpShopAjax(GrpKnd, resSelect);
}

function AddCommaDP(A, B) {
    NA = EleValue(A);
    if (NA != "" & NA != "0") {
        AddComma(A);
        SetValueSimplMain(B, '0');
    }
}
function DownloadPhoto(Area) {
    // GET CANVAS
    var canvas = document.getElementById("Picture");
    var ctx = canvas.getContext("2d");
    canvas.width = 600;
    canvas.height = 800;
    
    const tempImg = document.createElement('img');
    tempImg.src = 'data:image/svg+xml,' + encodeURIComponent(EleHtml(Area));

    ctx.drawImage(tempImg, 10, 10);
    // CREATE LINK
    let anchor = document.createElement("a");
    anchor.download = "download.jpg";
    anchor.href = canvas.toDataURL("image/jpg" );

    // "FORCE DOWNLOAD"
    anchor.click();
    anchor.remove();

}

function candown(target, type) {
    // (B1) GET CANVAS
    let canvas = document.getElementById(target);

    // (B2) CREATE LINK
    let anchor = document.createElement("a");
    anchor.download = "download." + type;
    anchor.href = canvas.toDataURL("image/" + type);

    // (B3) "FORCE DOWNLOAD"
    anchor.click();
    anchor.remove();

    // (B4) SAFER ALTERNATIVE - LET USER CLICK ON LINK
    // anchor.innerHTML = "Download";
    // document.body.appendChild(anchor);
}


function SetTiny(S) {
    const height45 = parseInt(0.45 * $(window).height());
    tinymce.remove('#' + S);

    tinymce.init({
        selector: 'textarea#' + S,
        license_key: 'gpl', // gpl for open source, T8LK:... for commercial
        font_size_formats: '8pt 10pt 12pt 14pt 16pt 18pt 20pt 24pt 28pt 32pt 36pt 40pt 48pt',
        width: '100%',
        resize: true,
        plugins:
            "advlist autolink autoresize directionality lists link image charmap preview anchor searchreplace visualblocks code fullscreen insertdatetime media table ",
        toolbar: " undo redo | blocks fontsize | bold italic | forecolor backcolor | ltr rtl alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image",
        relative_urls: false,
        remove_script_host: false,
        convert_urls: true,
        directionality: "rtl",
        promotion: false,
        min_height: height45,
    });


}

function GoToUrl(URL) {
    $('#waitbody').css('display', 'flex');
    window.location.href = URL;

    clearInterval(countdown);
    timeLeft = 0;
    countdown = setInterval(() => {
        timeLeft++;
        $('#timerWait').text(timeLeft);
        if (timeLeft >= 1000) {
            clearInterval(countdown);
        }
    }, 1000);

    return false;
}

function reloadPage() {
    timeLeft = 0;
    clearInterval(countdown);
    countdown = setInterval(() => {
        timeLeft++;
        $('#timerWait').text(timeLeft);
        if (timeLeft >= 1000) {
            clearInterval(countdown);
        }
    }, 1000);
    $('#waitbody').css('display', 'flex');
    window.location.reload();
}

function SelectItem(S, Fld, Area) {
    var ID = EleValue(S);
    var IDValue = ',' + ID + ',';
    var IdsValue = EleValue(Fld);
    if (IdsValue.includes(IDValue) == true)
        return;

    SetValueSimplMain(Fld, IdsValue + ID + ',');
    var fnTxt = "ReturnSelectItem('" + ID + "','" + Fld + "','" + Area + "');";

    var NewItem = '<div class="ListItemsSelected d-flex justify-content-between" id="' + Area + '_' + ID + '"><span>' + SelectOptionText(S) + '</span> <span class="icofont-2x icofont-delete text-danger btn-hover" onclick = "' + fnTxt + '" ></span> </div>'
    $('#' + Area).append(NewItem);
    SetValueSimplMain(S, '');
}

function ReturnSelectItem(ID, Fld, Area) {
    var IDValue = ',' + ID + ',';
    var IdsValue = EleValue(Fld);
    IdsValue = IdsValue.replace(IDValue, ',');
    SetValueSimplMain(Fld, IdsValue);
    $('#' + Area + '_' + ID).remove();
}
function AreaToggelWhitId(Area, Id) { (EleValue(Id) == "0") ? AreaShow(Area) : AreaHide(Area); }
function AreaToggelWhitIdNot(Area, Id) { (EleValue(Id) > "0") ? AreaShow(Area) : AreaHide(Area); }
function ActiveArea(AREA, ACTIVE) {
    $("#" + AREA).find('.AreaDetail').hide(500);
    $('#' + ACTIVE).show(500);
}
function ToggelYesNo(ELM, FLD) {
    var SRC = $(ELM).attr('src');
    if (EleValue(FLD) == "1") {
        SRC = SRC.replace('Btn_Yes', 'Btn_No');
        $(ELM).attr('src', SRC);
        SetValueSimpl(FLD, '0');
    }
    else {
        SRC = SRC.replace('Btn_No', 'Btn_Yes');
        $(ELM).attr('src', SRC);
        SetValueSimpl(FLD, '1');
    }
};

function ChangAllCass(AREA, DeActive, ACTIVE) {
    $("#" + AREA).find('.' + DeActive).addClass(ACTIVE).removeClass(DeActive);
}
function AddCunter(EL) { /* شمارنده*/ var Nq = parseInt(EleText(EL)); Nq += 1; SetTextMain(EL, Nq) }
function Focus(ELE) {
    document.getElementById(ELE).focus();
}
function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}
/*console.log("site.js END");*/
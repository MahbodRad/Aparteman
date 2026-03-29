SabtFactorComp_4 = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES.responseText)) {
        var JS = JSON.parse(RES.responseJSON);
        if (JS.res != "OK")
            ShowMsg('خطا در ثبت فاکتور', JS.restp);
        else {
            ShowMsg('ثبت فاکتور', JS.restp);

            SetHtml('AreaSabt', '<div class="alert alert-primary">ابتدا مشتری را انتخاب کنید<input type="hidden" name="Customer" value="0" /></div>')
        }

    }
}

SabtProductComp_4 = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES.responseText)) {
        var JS = JSON.parse(RES.responseJSON);
        if (JS.res != "OK")
            ShowMsg('خطا در ثبت فاکتور', JS.restp);
        else {
            ShowMsg('ثبت فاکتور', JS.restp);

            ClickBtn('RefreshProductBtn');
        }

    }
}

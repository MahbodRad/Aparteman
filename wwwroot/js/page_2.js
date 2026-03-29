SabtFactorComp_2 = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES.responseText)) {
        var JS = JSON.parse(RES.responseJSON);
        if (JS.res != "OK")
            ShowMsg('خطا در ثبت فاکتور', JS.restp);
        else {
            ShowMsg('ثبت فاکتور', JS.restp);

            SetHtml('AreaProducts', '<div class="alert alert-primary">برای سفارش جدید ابتدا کالاها را انتخاب کنید</div>')
        }

    }
}

function DissectionItem(ITEM) {
    SetText('ItemMethod', SelectOptionData(ITEM, 'data-Methode'), 'ItemPayer', SelectOptionData(ITEM, 'data-Payer'), 'ItemSharh', SelectOptionData(ITEM, 'data-Sharh'))
}
function DissectionItemRep(ITEM) {
    SetText('ItemMethodRep', SelectOptionData(ITEM, 'data-Methode'), 'ItemPayerRep', SelectOptionData(ITEM, 'data-Payer'), 'ItemSharhRep', SelectOptionData(ITEM, 'data-Sharh'))
}

function ShowTafzili(ITEM, BedBes) {
    const DetailCode = SelectOptionData(ITEM, 'data-Detail');

    AreaHide('tafzili' + BedBes + '_1');
    AreaHide('tafzili' + BedBes + '_2');
    AreaHide('tafzili' + BedBes + '_3');

    if (DetailCode == 4)
        AreaHide('tafzili' + BedBes + 'Area');
    else
        AreaShow('tafzili' + BedBes + 'Area');

    AreaShow('tafzili' + BedBes + '_' + DetailCode);
    SetValueSimpl('filterTafzili' + BedBes, '');
    SetHtml('ListFindTafzili' + BedBes, '<option value="0">در محدوده بالا جستجو کنید</option>');
}

function ShowTafziliRep(ITEM,REP) {
    const DetailCode = SelectOptionData(ITEM, 'data-Detail');
    AreaHide('tafziliRep' + REP +'_1');
    AreaHide('tafziliRep' + REP +'_2');
    AreaHide('tafziliRep' + REP +'_3');

    if (DetailCode == 4)
        AreaHide('tafziliRep' + REP +'Area');
    else
        AreaShow('tafziliRep' + REP +'Area');

    AreaShow('tafziliRep' + REP +'_' + DetailCode);
    SetValueSimpl('filterTafziliRep' + REP , '');
    SetHtml('ListFindtafziliRep' + REP , '<option value="0">در محدوده بالا جستجو کنید</option>');
}


FinancialEventsComplete = function (RES) {
    SpinnerHideForm();
    if (isJsonString(RES)) {
        const frm = RES.form
        const JS = RES.responseJSON;
        if (JS.success.toUpperCase() == "OK") {
            ClickBtnSimpl('FormSabtVoucherBtn', JS.FinancialEventId); 
            ShowMsg('افزودن ردیف ها', JS.message);
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

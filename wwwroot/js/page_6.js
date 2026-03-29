function DissectionItem(ITEM) {
    SetText('ItemMethod', SelectOptionData(ITEM, 'data-Methode'), 'ItemPayer', SelectOptionData(ITEM, 'data-Payer'), 'ItemSharh', SelectOptionData(ITEM, 'data-Sharh'))
}
function DissectionItemRep(ITEM) {
    SetText('ItemMethodRep', SelectOptionData(ITEM, 'data-Methode'), 'ItemPayerRep', SelectOptionData(ITEM, 'data-Payer'), 'ItemSharhRep', SelectOptionData(ITEM, 'data-Sharh'))
}

function ShowTafzili(ITEM, BedBes) {
    const DetailCode = SelectOptionData(ITEM, 'data-Detail');
    if (BedBes == 'Bed') {
        AreaHide('tafziliBed_1');
        AreaHide('tafziliBed_2');
        AreaHide('tafziliBed_3');

        if (DetailCode == 4)
            AreaHide('tafziliBedArea');
        else
            AreaShow('tafziliBedArea');

        AreaShow('tafziliBed_' + DetailCode);
        SetValueSimpl('ListFindTafziliCodeBed', DetailCode, 'filterTafziliBed', '');
        SetHtml('ListFindTafziliBed', '<option value="0">در محدوده بالا جستجو کنید</option>');
    }
    else {
        AreaHide('tafziliBes_1');
        AreaHide('tafziliBes_2');
        AreaHide('tafziliBes_3');

        if (DetailCode == 4)
            AreaHide('tafziliBesArea');
        else
            AreaShow('tafziliBesArea');

        AreaShow('tafziliBes_' + DetailCode);
        SetValueSimpl('ListFindTafziliCodeBes', DetailCode, 'filterTafziliBes', '');
        SetHtml('ListFindTafziliBes', '<option value="0">در محدوده بالا جستجو کنید</option>');
    }
}

function ShowTafziliRep(ITEM) {
    const DetailCode = SelectOptionData(ITEM, 'data-Detail');
    AreaHide('tafziliRep_1');
    AreaHide('tafziliRep_2');
    AreaHide('tafziliRep_3');

    if (DetailCode == 4)
        AreaHide('tafziliRepArea');
    else
        AreaShow('tafziliRepArea');

    AreaShow('tafziliRep_' + DetailCode);
    SetValueSimpl('ListFindTafziliCodeRep', DetailCode, 'filterTafziliRep', '');
    SetHtml('ListFindTafziliRep', '<option value="0">در محدوده بالا جستجو کنید</option>');
}

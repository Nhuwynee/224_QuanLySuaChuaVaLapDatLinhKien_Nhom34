var cartAttributes = {
};
                                                                                                                            
var addressArr = [
];
var locationHeader = true;
     
/*-----*/
function parseQueryString(url){
    var str = url != undefined? '?'+url.split('?')[1] : window.location.search;
    var objURL = {};
    var count = 0;
    if(str.indexOf("utm") > -1){
        count = 1;
    }
    str.replace(
        new RegExp("([^?=&]+)(=([^&]*))?", "g"),
        function($0, $1, $2, $3) {
            objURL[$1] = $3;
        }
    );
    return objURL;
};
var paramUrl = parseQueryString();
var days = {
    '1': 'Thứ 2',
    '2': 'Thứ 3',
    '3': 'Thứ 4',
    '4': 'Thứ 5',
    '5': 'Thứ 6',
    '6': 'Thứ 7',
    '0': 'Chủ nhật'
}

/*-----*/
var cartGet = null;
if (!$.isEmptyObject(paramUrl)) { // check paramUrl khác rỗng
    if (paramUrl.hasOwnProperty('f_fpi') && paramUrl.hasOwnProperty('f_pid') && paramUrl.hasOwnProperty('f_sig') && paramUrl.hasOwnProperty('f_tim')) {
        Cookies.set('gvn_chatbot','chatbotbilling');
        Cookies.set('gvn_channel_id','1');
        Cookies.set('gvn_page_id',paramUrl.f_fpi);
        Cookies.set('gvn_psid',paramUrl.f_pid);
        Cookies.set('gvn_signature',paramUrl.f_sig);
        Cookies.set('gvn_timestamp',paramUrl.f_tim);
    }
    if (paramUrl.hasOwnProperty('f_fpi') && paramUrl.hasOwnProperty('f_pid')) {
        $.get('/cart.js').done(function(data){ 
            cartGet = data;
            if (cartGet != null) {
                cartGet.attributes.X_Haravan_Social_PageId = paramUrl.f_fpi;
                cartGet.attributes.X_Haravan_Social_Psid = paramUrl.f_pid;
                cartGet.attributes.channel = paramUrl.channel;
                $.ajax({
                    url: '/cart/update.js',
                    type: 'POST',
                    data: {
                        "attributes": cartGet.attributes 
                    },
                    success: function(data){},
                    error: function(){}
                });
            }
        });
    }
}

/*-----*/
var store = [
         
             {
                "name": "HCM-HVT-KD",
                "province": "Hồ Chí Minh",
                "district": "Quận Phú Nhuận"     
             }	];
store = store.sort((a, b) => a.district < b.district ? -1 : 1);
var newStore = {};
store.map(x => {
    if(!newStore.hasOwnProperty(x.province)){
        newStore[x.province] = {};
        if(!newStore[x.province].hasOwnProperty(x.district)){
            newStore[x.province][x.district] = [];
            newStore[x.province][x.district].push(x);
        }
    }
    else{
        if(!newStore[x.province].hasOwnProperty(x.district)){
            newStore[x.province][x.district] = [];
            newStore[x.province][x.district].push(x);
        }
        else{
            newStore[x.province][x.district].push(x);
        }
    }
});

/*--*/
window.collecPagi = {
    current: paramUrl.page?parseInt(paramUrl.page):1,
    value: 100,
    itemRemove: 0
};
window.collecConfig = {
    scroll: 0,
    collection: {
        name: "",
        handle: "",
        id: "",
        url: "",
        totalProduct: null 
    }, 
    id: "",  
    filterData: {
        viewAll: false, //Hiển thị sản phẩm hết hàng
        pick: {
            viewAll: null,
            vendor: null,
            price: null,
            type: null,
            sortBy: null
        }
    },
    sortBy: null,
    defaultSortBy: ""	
};																												
                
window.shop = { 
    template: "index",
    moneyFormat: "{{amount}}₫",
    shopCurrency: "VND",
    CollectionSortBy: "",
    terms: "",
    web_url: 'gearvn.com',
    web_domain: 'gvnco.myharavan.com',
    shop_id: 200000722513,
    account: {
        logged: false,
        id: null,
        email: null,
        name: null,
        last_name:null,
        first_name:null,
        tag: ""
    },
    product: {
        data: null,
        id: null,
        handle: null,
        title: {"error":"json not allowed for this object"},
        type: null,
        vendor: null,
        sku: null,
        collection_title: [
            null
        ],
        collection_id: null,
        gift: false
    },
    favorites: [],
    profile: null,
    blog: {
        curl: null
    }
}
window.shop_app = {
    url_api: '//gvnco.myharavan.com/gvnes',
    key_api: '',
    secret:  '',

/* new update */
    apiCheckProfile:    '/apps/customer-verification/auth/api/account/info',

/* Chỉ dành khi chưa login */
    requestVerifyPhone: '/apps/customer-verification/auth/api/authentication/basic/phone/verification/request',
    confirmVerifyPhone: '/apps/customer-verification/auth/api/authentication/basic/phone/verification/confirm',
    requestVerifyEmail: '/apps/customer-verification/auth/api/authentication/basic/email/verification/request',
    confirmVerifyEmail: '/apps/customer-verification/auth/api/authentication/basic/email/verification/confirm',
    /* End: Chỉ dành khi chưa login */
    
    updateInfo:         '/apps/customer-verification/auth/api/account/update-basic-info',
    
    /* Sau khi đã login success */
    requestAuthPhone:   '/apps/customer-verification/auth/api/authentication/basic/phone/change/request',
    confirmOTPAuthPhone:'/apps/customer-verification/auth/api/authentication/basic/phone/change/confirm',
    
    requestAuthEmail:   '/apps/customer-verification/auth/api/authentication/basic/email/change/request',
    confirmOTPAuthEmail:'/apps/customer-verification/api/authentication/basic/email/verification/confirm',
    /* End: Sau khi đã login success*/

    apiCheckPhone: '/apps/customer-verification/api/verification/phone/check',
    apiRequestVerify: '/apps/customer-verification/api/verification/phone/request-verify',
    apiVerifyCode: '/apps/customer-verification/api/verification/phone/verify-code',
    apiSendOtp: '/apps/customer-verification/api/otp-authentication/phone/send-otp',
    apiLoginPhone: '/apps/customer-verification/api/otp-authentication/phone/login',
    
    urlFile: 'https://customer-rating-apps.haravan.com/api/buyer/upload/files',
    postRating: '/apps/customer_rating/product_rating',
    defaultRating: '/apps/customer_rating/content_rating/product?org_id=' + window.shop.shop_id,
    
    urlRatingUpload: 'https://customer-reviews-api.haravan.app/api/buyer/upload_image?source=website&org_id=' + window.shop.shop_id,
    postRatingStar: 'https://gearvn.com/apps/customerreviews/product_rating',
    
    productKeyCombo: '1106805577',
    apiComboList: 				 '/apps/promotion-enterprise/api/combo/list',
    apiBxsyListSuggest: 	 '/apps/promotion-enterprise/api/buy-x-select-y/list-suggestion',
    apiBxsyListApplyAble:  '/apps/promotion-enterprise/api/buy-x-select-y/list-applyable',
    apiBxsyCheckApplyAble: '/apps/promotion-enterprise/api/buy-x-select-y/check-applyable',
    apiBxsyListItemBuy:    '/apps/promotion-enterprise/api/buy-x-select-y/list-item-buy',
    apiGiftListSuggestion: '/apps/promotion-enterprise/api/gift/list-suggestion',
    
    apiDiscountListItemBuy: '/apps/promotion-enterprise/api/buy-and-discount/list-item-buy',
    apiDiscountListSuggestion: '/apps/promotion-enterprise/api/buy-and-discount/list-suggestion'
}
window.shop_settings = {
    freeship_promo: {
        show: true,
        hanmuc: parseInt("4000000"),
        texthanmuc: "4 triệu"
    },
    inventory: {
        show: false
    },
    popup_promotion: {
        show: false,
        minutes: "90",
        type: 'banner'
    },
    productPageCountdown: {
        showAll: false,
        showByColl: true
    },
    flashsale: {
        use_hideprice: true
    },
    productPageRelateColl: {
        show: false,
        limit: "3"
    }
}
window.shop_tracking = {};
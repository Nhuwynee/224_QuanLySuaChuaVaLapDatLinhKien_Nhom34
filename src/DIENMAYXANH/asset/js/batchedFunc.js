const createBatchedFunc = (func) => {
    let running = false;
    let callbacks = [];
    return (callback) => {
      callbacks.push(callback);
      if (running) return;
      running = true;
      func((...args) => {
        const tmp_callbacks = [...callbacks];
        running = false;
        callbacks = []; 
        for (const callback of tmp_callbacks) {
          callback(...args);
        }
      })
    }
  };
  
  const PE = {
      cart: null,
      getCart: createBatchedFunc(function(callback){
          $.get('/cart.js').done(function(cart){
              PE.cart = cart;
              callback();
          })
      }),
      cartView: function(properties){
          debugger
          let result = false;
          const keys = Object.keys(properties);
          const ignoreKeys = ['PE-gift-item ', 'PE-combo-item']
          keys.map(key=>{
              ignoreKeys.map(ignoreKey => {
                  if(key.startsWith(ignoreKey)){
                      result = true;
                      return;
                  }
              })
          });
          return result;
      },
      helper: {
          escaped_html: function(val) {
              const find_html_escape_amp_character = /&(?!(amp;)|(#39;)|(lt;)|(gt;))/g;
              return val
                  .replace(find_html_escape_amp_character, '&amp;')
                  .replace(/</g, '&lt;')
                  .replace(/>/g, '&gt;');
          }
      },
      normal: {
          remove: function(index, callback){
              PE.change({index, quantity: 0}, function(error){
                  return callback(error);
              });
          },
          change: function(cart, callback){
              PE.change({index: cart.index, quantity: cart.quantity}, function(error){
                  return callback(error);
              });
          },
          cartItems: function(){
              let results = [];
              const itemKeyGift = 'PE-gift-item ';
              const itemKeyCombo = 'PE-combo-item';
              const itemKeyBxSy = 'PE-bXsY-item-select';
              const itemKeyGiftCode = 'PE-gift-code-item';
              const cart = PE.cart;
              cart.items.map((item, index) =>{
                  item.index = index + 1;
                  const propertyKeys = Object.keys(item.properties) || [];
                  let isPE = false;
                  propertyKeys.map(key => {
                      if(key.startsWith(itemKeyGift) || key.startsWith(itemKeyCombo) || key.startsWith(itemKeyBxSy) || key === itemKeyGiftCode){
                          isPE = true;
                          return;
                      }
                  });
                  if(!isPE) results.push(item);
              })
              return results;
          },
          isNormalItem: function(item) {
              const itemKeyGift = 'PE-gift-item ';
              const itemKeyCombo = 'PE-combo-item';
              const itemKeyBxSy = 'PE-bXsY-item-select';
              return !Object.keys(item.properties).some(property => [itemKeyGift, itemKeyCombo, itemKeyBxSy].some(prefix => property.startsWith(prefix)));
          }
      },
      add: function(body, callback){
          $.ajax({
              url: '/cart/add.js',
              type: 'POST',
              data: body,
              success: function(){
                  return callback();
              },
              error: function(XMLHttpRequest, textStatus) {
                  return callback(textStatus);
              }
          });
      },
      change: function(cart, callback){
          $.ajax({
              url: '/cart/change.js',
              type: 'POST',
              data: {quantity: cart.quantity, line: cart.index},
         async: false,
              success: function(){
                  return callback();
              },
              error: function(XMLHttpRequest, textStatus) {
                  return callback(textStatus);
              }
          });
      },
      cloneCart: () => {
          return $.extend(true, {}, PE.cart);
      },
      generateVirtualCart: function(id, addQty) {
          const cart = PE.cloneCart();
          const product = PE.product;
          let price = 0;
          const index = cart.items.findIndex(item => item.variant_id == id && PE.normal.isNormalItem(item));
          if (index != -1) {
              price = addQty * cart.items[index].price;
              cart.items[index].line_price += price;
              cart.items[index].line_price_orginal += addQty * cart.items[index].price_original;
              cart.items[index].quantity += addQty;
          } else {
              let newLine = {};
              $.each(product.variants, function(index, variant){
                  if(variant.id == id) {
                      price = (variant.price)*addQty;
                      newLine.id = null;
                      newLine.product_id = product.id;
                      newLine.variant_id = variant.id;
                      newLine.not_allow_promotion = product.not_allow_promotion;
                      newLine.price = variant.price;
                      newLine.price_original = variant.compare_at_price;
                      newLine.properties = {};
                      newLine.quantity = addQty;
                  }
              });
              cart.items.push(newLine);
          }
          cart.item_count += addQty;
          cart.total_price += price;
          return cart;
      },
      combo: {
          get: function(id, callback) {
              $.get("/apps/promotion-enterprise/api/combo/" + id, function(data, status){
                  return callback(null, data);
              })
          },
          count: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/combo/count", query, function(data, status){
                  return callback(null, data);
              })
          },
          getByCode: function(code, callback) {
              $.get("/apps/promotion-enterprise/api/combo/get-by-code", { code }, function(data, status){
                  return callback(null, data);
              })
          },
          list: function(callback){
              $.get("/apps/promotion-enterprise/api/combo/list", function(data, status){
                  return callback(null, data);
              })
          },
          search: function(query, callback){
              $.get("/apps/promotion-enterprise/api/combo/search?", query, function(data, status){
                  return callback(null, data);
              })
          },
          getComboItem: function(combo) {
              const key = 'PE-combo-item';
              return PE.cart.items.find(item => {
                  if (item.properties[key] && item.properties[key].startsWith(`${combo.code} |`)) {
                      return true;
                  }
              });
          },
          add: function(combo, callback){
              const cart = PE.cart;
              const key = 'PE-combo-detail '+combo.code;
              const comboItem = PE.combo.getComboItem(combo);
              if(comboItem){
                  combo.quantity += Number(comboItem.quantity);
              }
  
              const properties = {};
              properties[combo.code] = combo.quantity;
              PE.add({
                      id: PE.virtual_id,
                      quantity: 1,
                      properties: {
                          "PE-combo-set": JSON.stringify(properties)
                      }
                  }, function(error){
                      return callback(error);
              });
          },
          set: function(combo, callback){
                  const properties = {};
                  properties[combo.code] = combo.quantity;
                  PE.add({
                          id: PE.virtual_id,
                          quantity: 1,
                          properties: {
                              "PE-combo-set": JSON.stringify(properties)
                          }
                      }, function(error){
                          return callback(error);
                  });
          },
          remove: function(combo, callback){
                  const properties = {};
                  properties[combo.code] = 0;
                  PE.add({
                          id: PE.virtual_id,
                          quantity: 1,
                          properties: {
                              "PE-combo-set": JSON.stringify(properties)
                          }
                      }, function(error){
                          return callback(error);
                  });
          },
          cartItems: function(){
              let result = {};
              const itemKey = 'PE-combo-item';
              const cart = PE.cart;
              const comboAttributes = {};
              Object.keys(cart.attributes).map(att=>{
                  const [title, code] = att.split(' ');
                  if(title.startsWith('PE-combo-detail')){
                      comboAttributes[code] = cart.attributes[att];
                  }
              })
              cart.items.map(item =>{
                  if(item.properties[itemKey]){
                      const [codeCombo, titleCombo] = item.properties[itemKey].split(' | ');
                      if(result[codeCombo] && result[codeCombo].items){
                          result[codeCombo].items.push(item);
                          result[codeCombo].line_price += item.line_price;
                      }else{
                          result[codeCombo] = {
                              code: codeCombo,
                              title: titleCombo,
                              quantity: comboAttributes[codeCombo],
                              line_price: item.line_price,
                              items : [item],
                          };
                      }
                  }
              });
              return result;
          },
          checkUpdateSuccess: function(combo, callback) {
              PE.getCart(() => {
                  const currentComboItem = PE.combo.getComboItem(combo);
                  if (!currentComboItem) {
                      return callback(false);
                  }
                  return callback(true);
              });
          },
          checkAvailableSlot: function({ code, is_limit_slots = null, quantity }, callback) {
              let has_enough_slots = true;
              if (!is_limit_slots && is_limit_slots != null) {
                  return callback(null, { has_enough_slots });
              }
              PE.combo.getByCode(code, (err, combo) => {
                  if (err) return callback(err);
                  if (combo.is_limit_slots && quantity > combo.available_slots) {
                      has_enough_slots = false;
                  }
                  return callback(null, { has_enough_slots });
              })
          }
      },
      gift: {
          get: function(id, callback) {
              $.get("/apps/promotion-enterprise/api/gift/" + id, function(data, status){
                  return callback(null, data);
              })
          },
          count: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/gift/count", query, function(data, status){
                  return callback(null, data);
              })
          },
          list: function(callback){
              $.get("/apps/promotion-enterprise/api/gift/list", function(data, status){
                  return callback(null, data);
              })
          },
          search: function(query, callback){
              $.get("/apps/promotion-enterprise/api/gift/search?", query, function(data, status){
                  return callback(null, data);
              })
          },
          getByCode: function(code, callback) {
              $.get("/apps/promotion-enterprise/api/gift/get-by-code", { code }, function(data, status){
                  return callback(null, data);
              })
          },
          cartItems: function(){
              let results = [];
              const itemKey = 'PE-gift-item ';
              const cart = PE.cart;
              cart.items.map(item =>{
                  const propertyKeys = Object.keys(item.properties) || [];
                  propertyKeys.map(key => {
                      if(key.startsWith(itemKey)){
                          item.gift = item.properties[key];
                          results.push(item);
                          return;
                      }
                  });
              })
              return results;
          },
      listSuggestion: function({ list_product_id, list_variant_id, fields }, callback) {
              $.ajax({
                  type: 'GET',
                  url: '/apps/promotion-enterprise/api/gift/list-suggestion',
                  dataType: 'json',
          data: { list_product_id, list_variant_id, fields },
                  success: function(res) {
                      callback(null, res);
                  },
                  error: function(err){
                      callback(err);
                  }
              })
          },
      listItemBuy: function({ id, code, fields }, callback) {
              $.ajax({
                  type: 'GET',
                  url: '/apps/promotion-enterprise/api/gift/list-item-buy',
                  dataType: 'json',
          data: { id, code, fields },
                  success: function(res) {
                      callback(null, res);
                  },
                  error: function(err){
                      callback(err);
                  }
              })
          },
      },
      bXsY: {
          count: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/buy-x-select-y/count", query, function(data, status){
                  return callback(null, data);
              })
          },
          search: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/buy-x-select-y/search", query, function(data, status){
                  return callback(null, data);
              })
          },
          detail: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/buy-x-select-y/detail", query, function(data, status){
                  return callback(null, data);
              })
          },
          listItemBuy: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/buy-x-select-y/list-item-buy", query, function(data, status){
                  return callback(null, data);
              })
          },
          checkApplyable: function(bXsY, callback) {
              const cart = PE.cart;
              const data = {'cart': cart, 'buy_x_select_y_id': bXsY.buy_x_select_y_id, 'buy_x_select_y_code': bXsY.buy_x_select_y_code };
              $.ajax({
                  type: 'POST',
                  data: JSON.stringify(data),
                  headers: {
                      'Content-Type': 'application/json'
                  },
                  url: '/apps/promotion-enterprise/api/buy-x-select-y/check-applyable',
                  dataType: 'json',
                  success: function(res) {
                      callback(null, res);
                  },
                  error: function(err){
                      callback(err);
                  }
              });
          },
          set: function (data, callback) {
              const properties = {};
              const data_set = { id: PE.virtual_id, quantity: 1 };
              properties['PE-bXsY-set'] = JSON.stringify(data);
              data_set['properties']= properties;
              PE.add(data_set, (err) => {
                  callback(err);
              });
          },
          listApplyable: createBatchedFunc(function(callback, options = {}) {
              const cart = options.cart || PE.cloneCart();
              cart.items.map(x => {
                  x.line_price = x.line_price/100;
                  x.line_price_orginal = x.line_price_orginal/100;
                  x.price = x.price/100;
                  x.price_original = x.price_original/100;
                  return x;
              })
              const data = {'cart': cart};
              $.ajax({
                  type: 'POST',
                  data: JSON.stringify(data),
                  headers: {
                      'Content-Type': 'application/json'
                  },
                  url: '/apps/promotion-enterprise/api/buy-x-select-y/list-applyable',
                  dataType: 'json',
                  success: function(res) {
                      if(res.success == true) {
                          PE.bXsY.list_applyable = res.data.items || [];
                      }
                      callback(null, res);
                  },
                  error: function(err){
                      callback(err);
                  }
              });
          }),
          listSuggestion: function(ids, callback) {
              $.ajax({
                  type: 'GET',
                  url: '/apps/promotion-enterprise/api/buy-x-select-y/list-suggestion?list_variant_id=' + ids,
                  dataType: 'json',
                  success: function(res) {
                      callback(null, res);
                  },
                  error: function(err){
                      callback(err);
                  }
              })
          },
          getListSelectedPromotionIdFromCart: function() {
              const cart = PE.cart;
              return cart.items.reduce((res, item) => {
                  const property = Object.keys(item.properties).find(key => key.startsWith('PE-bXsY-item-select'));
                  if (property) {
                      const tokens = item.properties[property].split('|');
                      const id = tokens[1].trim();
                      res.push(id);		
                  }
                  return res;
              }, []);
          },
          getVirtualListApplyable: function(id, addQty, callback) {
              PE.getCart(() => {
                  const list_selected = PE.bXsY.getListSelectedPromotionIdFromCart();
                  const list_applyable = [];
                  const cart = PE.generateVirtualCart(id, addQty);
                  PE.bXsY.listApplyable((err, res) => {
                      if (err) return callback(err);
                      if(res.success == true) {
                          const list = res.data.items || [];
                          if (list.length > 0) {
                              $.each(list, (index, value) => {
                                  $.each(value.list_condition_and_promotions, (index2, value2) => {
                                      const selected_limit = value2.list_promotion.filter(item => item.is_selected).length >= value2.max_selected_promotion;
                                      $.each(value2.list_promotion, (index3, value3) => {
                                          if (value2.is_matched_condition) {
                                              if (value3.is_selected || !selected_limit) {
                                                  list_applyable.push(value3._id);
                                              }
                                          } else {
                                              if (list_selected.includes(value3._id)) {
                                                  const matched_replace = PE.bXsY.getReplace(value, value3);
                                                  if (matched_replace) {
                                                      list_selected.push(matched_replace._id);
                                                  }
                                              }
                                          }
                                      });
                                  });
                              });
                          }
                      }
                      callback(null, { list_selected, list_applyable });
                  }, { cart })
              });
          },
          getReplace: function (bXsY, promotion) {
              for (const value of bXsY.list_condition_and_promotions) {
                  if (value.is_matched_condition) {
                      for (const value2 of value.list_promotion) {
                          if (value2._id != promotion._id && value2.list_items.length == promotion.list_items.length) {
                              let is_replaceable = true;
                              for (const value3 of promotion.list_items) {
                                  if (!value2.list_items.some(item => item.variant_id == value3.variant_id && item.quantity == value3.quantity && item.price == value3.price )) {
                                      is_replaceable = false;
                                      break;
                                  }
                              }
                              if (is_replaceable) {
                                  return value2;
                              }
                          }
                      }
                  }
              }
          },
          checkUpdateSuccess: function (list_updated_promotion, callback) {
              const list_out_of_stock = [];
              const list_out_of_limit = [];
              PE.getCart(() => {
                  const cart = PE.cart;
                  for (const updated of list_updated_promotion) {
                      const { id, code, quantity } = updated;
                      if (!cart.items.some(item => item.properties && item.properties['PE-bXsY-item-select ' + code])) {
                          list_out_of_stock.push({ code });
                      } else {
                          if (quantity != PE.bXsY.getQuantity(id, code)) {
                              list_out_of_limit.push({ id, code });
                          }
                      }
                  }
                  if (list_out_of_stock.length  || list_out_of_limit.length) {
                      return callback({ list_out_of_stock, list_out_of_limit });
                  }
                  callback(null);
              });
          },
          getQuantity: function(id, code) {
              const cart = PE.cart;
              const selected_promotions = cart.attributes['PE-bXsY-detail ' + code].split('|').pop().replace('SELECTED_PROMOTIONS :', '').split(',');
              const selected_promotion = selected_promotions.find(selected_promotion => selected_promotion.includes(id));
              const quantity = selected_promotion.split('x').pop().trim();
              return quantity;
          },
          cartItems: function(callback) {
              const res = {};
              const cart = PE.cart;
              $.each(cart.items, (index, item) => {
                  const property = Object.keys(item.properties).find(item => item.startsWith('PE-bXsY-item-select'));
                  if (property) {
                      const tokens = item.properties[property].split('|');
                      const title = tokens[0].trim();
                      const id = tokens[1].trim();
                      const code = property.replace('PE-bXsY-item-select ', '');
  
                      const quantity = PE.bXsY.getQuantity(id, code);
  
                      if (!res[code]) {
                          res[code] = {
                              code,
                              title,
                              promotions: {},
                          };
                      }
                      if (!res[code].promotions[id]) {
                          res[code].promotions[id] = {
                              quantity,
                              list_items: []
                          };
                      }
                      res[code].promotions[id].list_items.push(item);
                  }
              });
              return res;
          },
          remove: function(id, callback) {
              const data = {};
              const list_applyable = PE.bXsY.list_applyable;
              $.each(list_applyable, (index, value) => {
                  $.each(value.list_condition_and_promotions, (index2, value2) => {
                      $.each(value2.list_promotion, (index3, value3) => {
                          if (value3.is_selected) {
                              if (!data[value.code]) {
                                  data[value.code] = {
                                      rank: value2.condition_value,
                                      selected_promotions: [],
                                  };
                              }
                              if (value3._id != id) {
                                  data[value.code].selected_promotions.push({ id: value3._id, quantity: value3.selected_quantity });
                              } else {
                                  data[value.code].selected_promotions.push({ id: value3._id, quantity: 0 });
                              }
                          }
                      });
                  });
              });
              PE.bXsY.set(data, callback);
          },
          update: function(id, quantity, callback) {
              const data = {};
              const list_applyable = PE.bXsY.list_applyable;
              let err = null;
              for (const value of list_applyable) {
                  for (const value2 of value.list_condition_and_promotions) {
                      for (const value3 of value2.list_promotion) {
                          if (value3.is_selected) {
                              if (!data[value.code]) {
                                  data[value.code] = {
                                      rank: value2.condition_value,
                                      selected_promotions: [],
                                  };
                              }
                              if (value3._id != id) {
                                  data[value.code].selected_promotions.push({ id: value3._id, quantity: value3.selected_quantity });
                              } else {
                                  const updated_selected_promotion_quantity = value2.selected_promotion_quantity - value3.selected_quantity + quantity;
                                  if (quantity > value3.selectable_quantity || updated_selected_promotion_quantity > value2.selectable_promotion_quantity) {
                                      return callback({ error: true, code: 'ERR_OUT_OF_LIMIT' });
                                  } else {
                                      data[value.code].selected_promotions.push({ id: value3._id, quantity: quantity });
                                  }
                              }
                          }
                      }
                  }
              }
              PE.bXsY.set(data, callback);
          }
      },
      buyAndDiscount: {
          count: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/buy-and-discount/count", query, function(data, status){
                  return callback(null, data);
              })
          },
          search: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/buy-and-discount/search", query, function(data, status){
                  return callback(null, data);
              })
          },
          listSuggestion: function(ids, callback) {
              $.ajax({
                  type: 'GET',
                  url: '/apps/promotion-enterprise/api/buy-and-discount/list-suggestion?list_variant_id=' + ids,
                  dataType: 'json',
                  success: function(res) {
                      callback(null, res);
                  },
                  error: function(err){
                      callback(err);
                  }
              })
          },
          listApplyable: function(callback) {
              const cart = PE.cart;
              const data = {'cart': cart};
              $.ajax({
                  type: 'POST',
                  data: JSON.stringify(data),
                  headers: {
                      'Content-Type': 'application/json'
                  },
                  url: '/apps/promotion-enterprise/api/buy-and-discount/list-applyable',
                  dataType: 'json',
                  success: function(res) {
                      callback(null, res);
                  },
                  error: function(err){
                      callback(err);
                  }
              });
          },
          detail: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/buy-and-discount/detail", query, function(data, status){
                  return callback(null, data);
              })
          },
          listItemBuy: function(query, callback) {
              $.get("/apps/promotion-enterprise/api/buy-and-discount/list-item-buy", query, function(data, status){
                  return callback(null, data);
              })
          },
      },
      giftCode: {
          getApplyingGiftCode: function() {
              const itemKey = 'PE-gift-code-item';
              const item = PE.cart.items.find(item => Object.keys(item.properties).some(key => key === itemKey));
              if (item) {
                  const key = Object.keys(item.properties).find(key => key === itemKey);
                  return item.properties[key];
              }
              return null;
          },
          cartItems: function(){
              let res = {};
              const itemKey = 'PE-gift-code-item';
              const cart = PE.cart;
              cart.items.map(item =>{
                  const propertyKeys = Object.keys(item.properties) || [];
                  propertyKeys.map(key => {
                      if(key === itemKey){
                          const code = item.properties[key];
                          if (!res[code]) {
                              res[code] = {
                                  code,
                                  list_gift_items: []
                              };
                          }
                          res[code].list_gift_items.push(item);
                      }
                  });
              })
              return res;
          },
          add: function(code, callback) {
              const properties = {};
              properties['PE-gift-code-add'] = code;
              const data_add = { id: PE.virtual_id, quantity: 1 };
              data_add['properties']= properties;
              PE.add(data_add, (err) => {
                  callback(err);
              });
          },
          remove: function(code, callback) {
              const properties = {};
              properties['PE-gift-code-remove'] = code;
              const data_remove = { id: PE.virtual_id, quantity: 1 };
              data_remove['properties']= properties;
              PE.add(data_remove, (err) => {
                  callback(err);
              });
          },
          checkAddSuccess: function(code, callback) {
              PE.getCart(() => {
                  const gift_code = PE.giftCode.getApplyingGiftCode();
                  if (gift_code == code) {
                      return callback(true);
                  }
                  callback(false);
              });
          },
          getByCode: function(code, callback) {
              $.get("/apps/promotion-enterprise/api/gift-code/get-by-code", { code }, function(data, status){
                  return callback(null, data);
              })
          },
          checkApplyable: function(code, callback) {
              const cart = PE.cart;
              const data = {'cart': cart, 'gift_code_code': code };
              $.ajax({
                  type: 'POST',
                  data: JSON.stringify(data),
                  headers: {
                      'Content-Type': 'application/json'
                  },
                  url: '/apps/promotion-enterprise/api/gift-code/check-applyable',
                  dataType: 'json',
                  success: function(res) {
                      callback(null, res);
                  },
                  error: function(err){
                      callback(err);
                  }
              });
          }
      },
  }
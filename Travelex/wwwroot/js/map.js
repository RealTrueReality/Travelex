let map = null;
let marker = null;
let geocoder = null;
let autoComplete = null;
let dotNetHelper = null;

// 初始化地图
window.initAMap = async (helper) => {
    dotNetHelper = helper;

    // 加载高德地图脚本
    await loadAMapScript();

    // 创建地图实例
    map = new AMap.Map('map-container', {
        zoom: 11,
        center: [116.397428, 39.90923],
    });

    // 创建标记
    marker = new AMap.Marker({
        position: map.getCenter(),
        draggable: true,
        cursor: 'move',
        animation: 'AMAP_ANIMATION_BOUNCE'
    });

    marker.setMap(map);

    // 地图点击事件
    map.on('click', (e) => {
        marker.setPosition(e.lnglat);
        updateLocationInfo(e.lnglat);
    });

    // 标记拖拽结束事件
    marker.on('dragend', (e) => {
        updateLocationInfo(e.lnglat);
    });

    // 初始化地理编码服务
    geocoder = new AMap.Geocoder({
        city: "全国"
    });

    // 初始化搜索服务
    autoComplete = new AMap.AutoComplete({
        city: '全国'
    });
};

// 搜索位置
window.searchLocation = (keyword) => {
    autoComplete.search(keyword, (status, result) => {
        if (status === 'complete' && result.tips) {
            const locations = result.tips.map(tip => ({
                name: tip.name,
                address: tip.district + tip.address,
                latitude: tip.location?.lat || 0,
                longitude: tip.location?.lng || 0
            }));
            dotNetHelper.invokeMethodAsync('UpdateSearchResults', locations);
        }
    });
};

// 设置地图中心点
window.setMapCenter = (lat, lng) => {
    const position = new AMap.LngLat(lng, lat);
    map.setCenter(position);
    marker.setPosition(position);
    updateLocationInfo(position);
};

// 获取当前位置信息
window.getCurrentLocation = () => {
    return new Promise((resolve) => {
        const position = marker.getPosition();
        geocoder.getAddress([position.getLng(), position.getLat()], (status, result) => {
            if (status === 'complete' && result.regeocode) {
                resolve({
                    name: result.regeocode.formattedAddress,
                    address: result.regeocode.addressComponent.district + result.regeocode.addressComponent.township,
                    latitude: position.getLat(),
                    longitude: position.getLng()
                });
            }
        });
    });
};

// 更新位置信息
function updateLocationInfo(lnglat) {
    geocoder.getAddress([lnglat.getLng(), lnglat.getLat()], (status, result) => {
        if (status === 'complete' && result.regeocode) {
            // 可以在这里添加额外的位置信息处理逻辑
        }
    });
}

// 加载高德地图脚本
function loadAMapScript() {
    return new Promise((resolve, reject) => {
        if (window.AMap) {
            resolve();
            return;
        }

        const script = document.createElement('script');
        script.src = 'https://webapi.amap.com/maps?v=2.0&key=YOUR_AMAP_KEY&plugin=AMap.Geocoder,AMap.AutoComplete';
        script.async = true;
        script.onload = () => resolve();
        script.onerror = () => reject();
        document.head.appendChild(script);
    });
}

let map = null;
let marker = null;
let geocoder = null;
let autoComplete = null;
let dotNetHelper = null;
let geolocation = null;

// 初始化地图
window.initAMap = async (helper) => {
    try {
        dotNetHelper = helper;

        // 加载高德地图脚本
        await loadAMapScript();

        // 设置安全密钥
        window._AMapSecurityConfig = {
            securityJsCode: '91413e848de640e2734f9b42857bb255',
        };

        // 如果已经存在地图实例，先销毁
        if (map) {
            map.destroy();
            map = null;
        }

        // 创建地图实例
        map = new AMap.Map('map-container', {
            zoom: 11,
        });

        // 初始化定位插件
        await initGeolocation();

        // 创建标记
        marker = new AMap.Marker({
            position: map.getCenter(),
            draggable: true,
            cursor: 'move',
            animation: 'AMAP_ANIMATION_BOUNCE'
        });

        marker.setMap(map);

        // 地图点击事件
        map.on('click', handleMapClick);
        
        // 标记拖拽结束事件
        marker.on('dragend', handleMarkerDragend);

        // 初始化地理编码服务
        geocoder = new AMap.Geocoder({
            city: "全国",
            batch: false,
            extensions: "all"
        });

        // 初始化搜索服务
        autoComplete = new AMap.AutoComplete({
            city: '全国'
        });
    } catch (error) {
        console.error('Failed to initialize AMap:', error);
        await dotNetHelper.invokeMethodAsync('HandleMapError', 'Failed to initialize map');
    }
};

// 地图点击事件处理函数
const handleMapClick = (e) => {
    marker.setPosition(e.lnglat);
    updateLocationInfo(e.lnglat);
};

// 标记拖拽结束事件处理函数
const handleMarkerDragend = (e) => {
    updateLocationInfo(e.lnglat);
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
    return new Promise((resolve, reject) => {
        try {
            if (!marker) {
                reject('Marker not initialized');
                return;
            }
            
            const position = marker.getPosition();
            if (!position) {
                reject('No position available');
                return;
            }

            console.log('Getting location for position:', position.toString());
            
            // 使用逆地理编码获取地址信息
            geocoder.getAddress([position.getLng(), position.getLat()], (status, result) => {
                console.log('Geocoder response:', status, result);
                
                if (status === 'complete' && result.regeocode) {
                    const addressComponent = result.regeocode.addressComponent;
                    const pois = result.regeocode.pois || [];
                    
                    // 优先使用最近的POI名称
                    let locationName = '';
                    if (pois.length > 0) {
                        locationName = pois[0].name;
                    }

                    // 如果没有POI，则使用简化的地址
                    if (!locationName) {
                        const components = [];
                        if (addressComponent.district) components.push(addressComponent.district);
                        if (addressComponent.township) components.push(addressComponent.township);
                        if (addressComponent.street) components.push(addressComponent.street);
                        if (addressComponent.streetNumber) components.push(addressComponent.streetNumber);
                        locationName = components.join('');
                    }

                    const locationResult = {
                        name: locationName || result.regeocode.formattedAddress,
                        // 保存完整地址作为详细信息
                        address: result.regeocode.formattedAddress,
                        latitude: position.getLat(),
                        longitude: position.getLng()
                    };
                    console.log('Location result:', locationResult);
                    resolve(locationResult);
                } else {
                    console.warn('Geocoder failed, using coordinates as fallback');
                    // 如果逆地理编码失败，尝试再次获取
                    setTimeout(() => {
                        geocoder.getAddress([position.getLng(), position.getLat()], (retryStatus, retryResult) => {
                            if (retryStatus === 'complete' && retryResult.regeocode) {
                                const pois = retryResult.regeocode.pois || [];
                                let locationName = '';
                                if (pois.length > 0) {
                                    locationName = pois[0].name;
                                }

                                resolve({
                                    name: locationName || retryResult.regeocode.formattedAddress,
                                    address: retryResult.regeocode.formattedAddress,
                                    latitude: position.getLat(),
                                    longitude: position.getLng()
                                });
                            } else {
                                // 如果重试也失败了，才使用坐标
                                resolve({
                                    name: '未知地点',
                                    address: `${position.getLat().toFixed(6)}, ${position.getLng().toFixed(6)}`,
                                    latitude: position.getLat(),
                                    longitude: position.getLng()
                                });
                            }
                        });
                    }, 1000); // 等待1秒后重试
                }
            });
        } catch (error) {
            console.error('Error in getCurrentLocation:', error);
            const position = marker.getPosition();
            resolve({
                name: '未知地点',
                address: `${position.getLat().toFixed(6)}, ${position.getLng().toFixed(6)}`,
                latitude: position.getLat(),
                longitude: position.getLng()
            });
        }
    });
};

// 检查定位条件
async function checkLocationPermission() {
    // 检查设备是否支持地理定位
    if (!navigator.geolocation) {
        throw new Error('您的设备不支持地理定位');
    }

    // 通过 .NET MAUI 检查和请求权限
    if (dotNetHelper) {
        const hasPermission = await dotNetHelper.invokeMethodAsync('CheckLocationPermission');
        if (!hasPermission) {
            throw new Error('未获得位置权限');
        }
    }

    // 检查GPS是否开启（仅适用于Android）
    if (navigator.userAgent.toLowerCase().indexOf('android') > -1) {
        try {
            const result = await new Promise((resolve) => {
                AMap.plugin('AMap.Geolocation', () => {
                    const tempGeo = new AMap.Geolocation({
                        enableHighAccuracy: true,
                        timeout: 2000
                    });
                    tempGeo.getCurrentPosition((status, data) => {
                        resolve({ status, data });
                    });
                });
            });

            if (result.status === 'error' && result.data && result.data.message && result.data.message.indexOf('定位失败') > -1) {
                throw new Error('请开启GPS定位服务');
            }
        } catch (error) {
            throw new Error('请开启GPS定位服务');
        }
    }
}

// 初始化定位插件
function initGeolocation() {
    return new Promise((resolve, reject) => {
        AMap.plugin('AMap.Geolocation', () => {
            geolocation = new AMap.Geolocation({
                enableHighAccuracy: true, // 使用高精度定位
                timeout: 10000,          // 超时时间
                offset: [10, 20],        // 偏移量
                zoomToAccuracy: true,    // 定位成功后调整地图视野范围
                position: 'RB',          // 定位按钮位置
                buttonPosition: 'RB',     // 定位按钮位置
                buttonOffset: new AMap.Pixel(10, 20), // 定位按钮偏移量
                showButton: true,        // 显示定位按钮
                showMarker: false,       // 不显示定位点
                showCircle: false,       // 不显示精度圈
                panToLocation: true,     // 定位成功后将定位到定位点
                GeoLocationFirst: true   // 优先使用浏览器定位
            });

            map.addControl(geolocation);

            // 监听定位按钮点击事件
            geolocation.on('complete', onLocationComplete);
            geolocation.on('error', onLocationError);

            resolve();
        });
    });
}

// 定位成功回调
async function onLocationComplete(data) {
    try {
        console.log('Location success:', data);
        // 检查定位精度
        if (data.accuracy > 1000) { // 精度超过1000米
            throw new Error('定位精度较低，请检查GPS信号');
        }

        const position = [data.position.lng, data.position.lat];
        map.setCenter(position);
        if (marker) {
            marker.setPosition(position);
            await updateLocationInfo(new AMap.LngLat(position[0], position[1]));
        }
    } catch (error) {
        onLocationError(error);
    }
}

// 定位失败回调
async function onLocationError(error) {
    console.error('Location error:', error);
    let errorMessage = '获取位置失败';

    try {
        // 检查定位条件
        await checkLocationPermission();
        
        // 如果权限检查通过，但仍然失败，可能是其他原因
        if (error.info) {
            // AMap 错误
            if (error.info.indexOf('FAILED') > -1) {
                errorMessage = '定位失败，请检查GPS是否开启';
            } else if (error.info.indexOf('TIMEOUT') > -1) {
                errorMessage = '定位超时，请检查网络连接';
            } else {
                errorMessage = error.info;
            }
        } else if (error.code) {
            // HTML5 Geolocation 错误
            switch(error.code) {
                case error.PERMISSION_DENIED:
                    errorMessage = '请允许使用位置信息';
                    break;
                case error.POSITION_UNAVAILABLE:
                    errorMessage = '无法获取当前位置';
                    break;
                case error.TIMEOUT:
                    errorMessage = '获取位置超时';
                    break;
                default:
                    errorMessage = '定位失败，请稍后重试';
            }
        }
    } catch (permError) {
        errorMessage = permError.message;
    }

    try {
        if (dotNetHelper) {
            await dotNetHelper.invokeMethodAsync('HandleMapError', errorMessage);
        }
    } catch (invokeError) {
        console.error('Failed to invoke HandleMapError:', invokeError);
        // 如果调用失败，使用alert作为后备方案
        alert(errorMessage);
    }
}

// 更新位置信息
function updateLocationInfo(lnglat) {
    geocoder.getAddress([lnglat.getLng(), lnglat.getLat()], (status, result) => {
        if (status === 'complete' && result.regeocode) {
            const pois = result.regeocode.pois || [];
            let locationName = '';
            if (pois.length > 0) {
                locationName = pois[0].name;
            }

            // 如果没有POI，则使用简化的地址
            if (!locationName) {
                const addressComponent = result.regeocode.addressComponent;
                const components = [];
                if (addressComponent.district) components.push(addressComponent.district);
                if (addressComponent.township) components.push(addressComponent.township);
                if (addressComponent.street) components.push(addressComponent.street);
                if (addressComponent.streetNumber) components.push(addressComponent.streetNumber);
                locationName = components.join('');
            }

            const locationResult = {
                name: locationName || result.regeocode.formattedAddress,
                address: result.regeocode.formattedAddress,
                latitude: lnglat.getLat(),
                longitude: lnglat.getLng()
            };
            console.log('Location result:', locationResult);
        }
    });
}

// 清理地图资源
window.destroyMap = () => {
    if (map) {
        if (geolocation) {
            geolocation.off('complete', onLocationComplete);
            geolocation.off('error', onLocationError);
            map.removeControl(geolocation);
            geolocation = null;
        }
        
        map.off('click', handleMapClick);
        if (marker) {
            marker.off('dragend', handleMarkerDragend);
            marker.setMap(null);
            marker = null;
        }
        map.destroy();
        map = null;
    }
    if (geocoder) {
        geocoder = null;
    }
    if (autoComplete) {
        autoComplete = null;
    }
    if (dotNetHelper) {
        dotNetHelper = null;
    }
};

// 加载高德地图脚本
function loadAMapScript() {
    return new Promise((resolve, reject) => {
        if (window.AMap) {
            resolve();
            return;
        }

        // 设置安全密钥配置
        window._AMapSecurityConfig = {
            securityJsCode: '91413e848de640e2734f9b42857bb255',
        };

        const script = document.createElement('script');
        script.src = 'https://webapi.amap.com/maps?v=2.0&key=0ea3d0f98d99e3fb713de48fdd9a6046&plugin=AMap.Geocoder,AMap.AutoComplete,AMap.Geolocation';
        script.async = true;
        script.onload = () => resolve();
        script.onerror = () => reject();
        document.head.appendChild(script);
    });
}

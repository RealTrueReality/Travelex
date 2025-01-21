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

    // 检查网络连接
    if (!navigator.onLine) {
        throw new Error('请检查网络连接');
    }

    // 通过 .NET MAUI 检查和请求权限
    if (dotNetHelper) {
        try {
            const hasPermission = await dotNetHelper.invokeMethodAsync('CheckLocationPermission');
            console.log('Location permission check result:', hasPermission);
            if (!hasPermission) {
                throw new Error('未获得位置权限，请在系统设置中允许使用位置信息');
            }
        } catch (error) {
            console.error('Error checking permission:', error);
            throw new Error('检查位置权限失败');
        }
    }
}

// 初始化定位插件
function initGeolocation() {
    return new Promise((resolve, reject) => {
        AMap.plugin('AMap.Geolocation', () => {
            geolocation = new AMap.Geolocation({
                enableHighAccuracy: true,    // 使用高精度定位
                timeout: 20000,              // 增加超时时间到20秒
                buttonPosition: 'RB',        // 定位按钮位置
                buttonOffset: new AMap.Pixel(10, 20), // 定位按钮偏移量
                showButton: true,            // 显示定位按钮
                showMarker: true,            // 显示定位点
                showCircle: true,            // 显示精度圈
                panToLocation: true,         // 定位成功后将定位到定位点
                GeoLocationFirst: true,      // 优先使用浏览器定位
                useNative: true              // 使用安卓定位sdk
            });

            map.addControl(geolocation);
            
            // 先检查权限
            checkLocationPermission().then(() => {
                // 立即尝试定位
                geolocation.getCurrentPosition((status, result) => {
                    console.log('Initial location attempt:', status, result);
                    if (status === 'complete') {
                        onLocationComplete(result);
                    } else {
                        onLocationError(result);
                    }
                });

                // 监听定位按钮点击事件
                geolocation.on('complete', onLocationComplete);
                geolocation.on('error', onLocationError);
            }).catch(error => {
                console.error('Permission check failed:', error);
                onLocationError(error);
            });

            resolve();
        });
    });
}

// 定位成功回调
async function onLocationComplete(data) {
    try {
        console.log('Location success:', data);
        // 放宽精度限制到2000米
        if (data.accuracy > 2000) {
            console.warn('定位精度较低，但仍继续使用');
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
    console.error('Location error details:', {
        error: error,
        message: error.message || error.info,
        code: error.code,
        info: error.info,
        type: error.type,
        status: error.status
    });

    let errorMessage = '获取位置失败';

    try {
        if (error.code) {
            // HTML5 Geolocation 错误
            switch(error.code) {
                case 1: // PERMISSION_DENIED
                    errorMessage = '请允许使用位置信息';
                    break;
                case 2: // POSITION_UNAVAILABLE
                    errorMessage = '无法获取当前位置，请确保GPS已开启';
                    break;
                case 3: // TIMEOUT
                    errorMessage = '获取位置超时，请检查网络连接';
                    break;
            }
        } else if (error.info) {
            if (error.info.indexOf('FAILED') > -1) {
                errorMessage = '定位失败，请检查GPS是否开启并确保在室外开阔场地';
            } else if (error.info.indexOf('TIMEOUT') > -1) {
                errorMessage = '定位超时，请检查网络连接并重试';
            } else {
                errorMessage = error.info;
            }
        } else if (error.message) {
            errorMessage = error.message;
        }

        // 检查是否是权限问题
        if (errorMessage.includes('权限') || error.message?.includes('permission')) {
            await checkLocationPermission();
        }
    } catch (permError) {
        errorMessage = permError.message;
    }

    if (dotNetHelper) {
        await dotNetHelper.invokeMethodAsync('HandleMapError', errorMessage);
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

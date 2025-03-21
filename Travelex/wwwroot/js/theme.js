// 主题管理相关函数
window.setDarkMode = function (isDark) {
    if (isDark) {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
    
    // 处理MAUI原生部分
    if (window.location.hostname === 'localhost') {
        try {
            // 调用MAUI原生方法更新状态栏和导航栏颜色
            if (window.MauiApp && window.MauiApp.setNativeTheme) {
                window.MauiApp.setNativeTheme(isDark);
            }
        } catch (e) {
            console.error('Failed to set native theme:', e);
        }
    }
};

// 检测系统主题是否为暗色模式
window.isSystemInDarkMode = function () {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
};

// 设置系统主题变化监听器
window.setupSystemThemeListener = function () {
    if (window.matchMedia) {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        
        // 检测变化并通知.NET
        const handleChange = (e) => {
            if (window.DotNet) {
                try {
                    DotNet.invokeMethodAsync('Travelex', 'OnSystemThemeChanged', e.matches);
                } catch (e) {
                    console.error('Failed to invoke OnSystemThemeChanged:', e);
                }
            }
        };
        
        // 添加监听器
        if (mediaQuery.addEventListener) {
            mediaQuery.addEventListener('change', handleChange);
        } else {
            // 兼容旧版浏览器
            mediaQuery.addListener(handleChange);
        }
    }
};

// 在页面加载前应用保存的主题设置
(function initializeTheme() {
    try {
        // 检查本地存储中的主题偏好
        const themePreference = localStorage.getItem('themePreference');
        let shouldApplyDark = false;
        
        if (themePreference === 'Dark') {
            // 用户选择了深色模式
            shouldApplyDark = true;
        } else if (themePreference === 'Light') {
            // 用户选择了浅色模式
            shouldApplyDark = false;
        } else {
            // 用户选择了跟随系统或未设置，检测系统主题
            shouldApplyDark = window.matchMedia && 
                                window.matchMedia('(prefers-color-scheme: dark)').matches;
        }
        
        // 立即应用主题，不等待页面完全加载
        if (shouldApplyDark) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
    } catch (e) {
        console.error('主题初始化失败:', e);
    }
})();

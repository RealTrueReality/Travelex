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

window.scrollToBottom = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.copyToClipboard = async function (text) {
    try {
        // 创建一个临时输入框
        const input = document.createElement('input');
        input.setAttribute('readonly', '');
        input.setAttribute('value', text);
        input.style.position = 'absolute';
        input.style.left = '-9999px';
        document.body.appendChild(input);
        
        // 在移动设备上，需要特殊处理选择范围
        input.setSelectionRange(0, input.value.length);
        input.select();
        
        // 尝试复制
        document.execCommand('copy');
        document.body.removeChild(input);
        return true;
    } catch (err) {
        console.error('复制失败:', err);
        return false;
    }
};

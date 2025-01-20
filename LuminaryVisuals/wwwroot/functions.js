window.saveAsFile = (filename, base64Content) => {
    const byteCharacters = atob(base64Content);
    const byteNumbers = new Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'text/csv' });

    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
window.getTimezoneOffset = function () {
    return -new Date().getTimezoneOffset(); // Offset in minutes, negative for UTC-
};


function showLoadingIndicator() {
    const overlay = document.getElementById("loading-overlay");
    if (overlay) {
        overlay.style.display = "block";
    }
}

function hideLoadingIndicator() {
    const overlay = document.getElementById("loading-overlay");
    if (overlay) {
        overlay.style.display = "none";
    }
}

let isSvgShifted = false;
window.addSvgToTable = function (columnText1) {  // Add columnText1 parameter
    const tableContainer = document.querySelector('.mud-table-container');
    if (!tableContainer) return;

    // Get the position of the first column
    const targetSpan1 = Array.from(document.querySelectorAll('.sortable-column-header'))
        .find(span => span.textContent.trim() === columnText1);
    if (!targetSpan1) {
        console.error('First column not found');
        return;
    }

    const thElement1 = targetSpan1.closest('th');
    const rect1 = thElement1.getBoundingClientRect();
    const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svg.setAttribute('style', `position: absolute; top: 0; left: ${rect1.left + scrollLeft}px;margin-left:100px; width: 80%; pointer-events: all; transform:scale(1.42); z-index: 13;`);
    svg.setAttribute('class', 'connection-lines');

    // Rest of your existing addSvgToTable code...
    svg.addEventListener('click', async function (e) {
        const dotNetReference = await window.getDotNetReference();
        if (dotNetReference) {
            await dotNetReference.invokeMethodAsync('HandleSvgClick');
        }
        const screenWidth = window.innerWidth;
        // Toggle the margin-left on each click
        if (isSvgShifted) {
            svg.style.marginLeft = '80px';  
        }
        else {
            if (screenWidth > 960) {
                svg.style.marginLeft = '-538px';  // Margin for desktop view
            }
            else {
                svg.style.marginLeft = '-168px';
            }
        }
        isSvgShifted = !isSvgShifted; 
    });

    const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
    path.setAttribute('fill', 'none');
    path.setAttribute('stroke', 'white');
    path.setAttribute('stroke-width', '2');
    path.setAttribute('id', 'connection-path');

    svg.appendChild(path);
    tableContainer.insertBefore(svg, tableContainer.firstChild);
}
let dotNetReference = null;
window.setDotNetReference = function (reference) {
    dotNetReference = reference;
}

window.getDotNetReference = function () {
    return dotNetReference;
}
window.getColumnPosition = function (columnText1, columnText2) {
    const targetSpan1 = Array.from(document.querySelectorAll('.sortable-column-header'))
        .find(span => span.textContent.trim() === columnText1);
    const targetSpan2 = Array.from(document.querySelectorAll('.sortable-column-header'))
        .find(span => span.textContent.trim() === columnText2);
    if (!targetSpan1 || !targetSpan2) {
        console.error(`Columns not found`);
        return null;
    }
    const thElement1 = targetSpan1.closest('th');
    const thElement2 = targetSpan2.closest('th');
    if (!thElement1 || !thElement2) {
        console.error(`Parent th elements not found`);
        return null;
    }
    const rect1 = thElement1.getBoundingClientRect();
    const rect2 = thElement2.getBoundingClientRect();
    const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    return {
        start: {
            left: rect1.left + scrollLeft,
            top: rect1.top + scrollTop,
            right: rect1.right + scrollLeft,
            bottom: rect1.bottom + scrollTop,
            width: rect1.width,
            height: rect1.height
        },
        end: {
            left: rect2.left + scrollLeft,
            top: rect2.top + scrollTop,
            right: rect2.right + scrollLeft,
            bottom: rect2.bottom + scrollTop,
            width: rect2.width,
            height: rect2.height
        }
    };
};

window.updateSvgPath = function (columnText1, columnText2, labelText = '') {
    const positions = window.getColumnPosition(columnText1, columnText2);
    if (!positions) return;

    const path = document.getElementById('connection-path');
    const svg = path?.closest('svg');
    if (!path || !svg) return;

    // Calculate min/max bounds
    const minX = Math.min(positions.start.left, positions.end.left);
    const maxX = Math.max(positions.start.left, positions.end.left);
    const minY = Math.min(positions.start.top, positions.end.top) - 20;
    const maxY = Math.max(positions.start.top, positions.end.top);

    // Calculate middle point for label
    const middleX = minX + (maxX - minX) / 2;

    // Create text element
    let labelElement = svg.querySelector('#connection-label');
    if (!labelElement) {
        labelElement = document.createElementNS("http://www.w3.org/2000/svg", "text");
        labelElement.setAttribute('id', 'connection-label');
        labelElement.setAttribute('text-anchor', 'middle');
        labelElement.setAttribute('fill', 'white');
        labelElement.setAttribute('font-size', '12px');

        // Add both arrows to the label (inside the SVG)
        svg.appendChild(labelElement);
    }
    else {
        leftArrow = labelElement.querySelector('polygon:first-child');
        rightArrow = labelElement.querySelector('polygon:last-child');
    }

    // Positioning the label
    labelElement.setAttribute('x', middleX);
    labelElement.setAttribute('y', minY - 5); // 5px above the line
    labelElement.textContent = labelText;
 
    // Updating path
    const pathData = `M ${positions.start.left} ${positions.start.top} ` +
        `L ${positions.start.left} ${minY} ` +
        `L ${positions.end.left} ${minY} ` +
        `L ${positions.end.left} ${positions.end.top}`;
    path.setAttribute('d', pathData);

    // Adjusts the SVG to fit the path and label
    requestAnimationFrame(() => {
        const padding = 50;
        const width = maxX - minX + padding;
        const height = maxY - minY + padding;

        svg.setAttribute('width', width);
        svg.setAttribute('height', height);
        svg.setAttribute('viewBox', `${minX - padding / 2} ${minY - padding / 2} ${width} ${height}`);
    });
};

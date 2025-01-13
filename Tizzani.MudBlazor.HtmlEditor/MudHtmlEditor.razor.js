var Embed = Quill.import('blots/block/embed');

class Divider extends Embed {
    static create(value) {
        let node = super.create(value);
        node.setAttribute('style', "height: 0px; margin-top: 0.5em 0; border-width; 1px; border-style: solid none none none;");
        return node;
    }
}

Divider.blotName = 'hr';
Divider.tagName = 'hr';
Quill.register(Divider, true);

try {
    Quill.register('modules/blotFormatter', QuillBlotFormatter.default);
} catch { }    




export function createQuillInterop(dotNetRef, editorRef, toolbarRef, placeholder, theme) {

    // Configure modules based on theme
    let modules = {
        blotFormatter: {},
    };


    // Only add toolbar for snow theme
    if (theme === 'snow') {
        modules.toolbar = {
            container: toolbarRef
        };
    }

    var quill = new Quill(editorRef, {
        modules: modules,
        placeholder: placeholder,
        theme: theme
    });

    var Font = Quill.import('formats/font');
    Font.whitelist = ['roboto', 'open-sans', 'mycustomfont'];

    // Only add font handler for snow theme
    if (theme === 'snow') {
        quill.getModule('toolbar').addHandler('font', function (value) {
            quill.format('font', value);
        });
    }


    return new MudQuillInterop(dotNetRef, quill, editorRef, toolbarRef);
}

export class MudQuillInterop {
    /**
     * @param {Quill} quill
     * @param {Element} editorRef
     * @param {Element} toolbarRef
     */
    constructor(dotNetRef, quill, editorRef, toolbarRef) {
        quill.getModule('toolbar').addHandler('hr', this.insertDividerHandler);
        quill.on('text-change', this.textChangedHandler);
        this.dotNetRef = dotNetRef;
        this.quill = quill;
        this.editorRef = editorRef;
        this.toolbarRef = toolbarRef;
    }

    getText = () => {
        return this.quill.getText();
    };

    getHtml = () => {
        return this.quill.root.innerHTML;
    };

    setHtml = (html) => {
        this.quill.root.innerHTML = html;
    }

    insertDividerHandler = () => {
        const range = this.quill.getSelection();

        if (range) {
            this.quill.insertEmbed(range.index, "hr", "null");
        }
    };

    /**
     * 
     * @param {Delta} delta
     * @param {Delta} oldDelta
     * @param {any} source
     */
    textChangedHandler = (delta, oldDelta, source) => {
        this.dotNetRef.invokeMethodAsync('HandleHtmlContentChanged', this.getHtml());
        this.dotNetRef.invokeMethodAsync('HandleTextContentChanged', this.getText());
    };
}
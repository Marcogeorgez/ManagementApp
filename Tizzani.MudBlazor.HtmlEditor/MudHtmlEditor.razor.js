var Embed = Quill.import('blots/block/embed');
class ImageUploader {
    constructor(quill, options) {
        this.quill = quill;
        this.options = options;

        // If upload function was provided
        if (typeof this.options.upload === 'function') {
            this.upload = this.options.upload;
        }
    }
}


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
// Register the new module before creating the Quill instance
Quill.register('modules/imageUploader', ImageUploader, true);
try {
    Quill.register('modules/blotFormatter', QuillBlotFormatter.default);
} catch { }    




export function createQuillInterop(dotNetRef, editorRef, toolbarRef, placeholder, theme) {

    // Configure modules based on theme
    let modules = {
        blotFormatter: {},
        imageUploader: {
            upload: (file) => {
                return new Promise(async (resolve, reject) => {
                    try {
                        const reader = new FileReader();
                        reader.onload = async (e) => {
                            try {
                                const base64 = e.target.result;
                                const result = await dotNetRef.invokeMethodAsync('HandleImageUpload', base64, file.name);
                                resolve(result);
                            } catch (error) {
                                reject(error);
                            }
                        };
                        reader.onerror = () => reject(reader.error);
                        reader.readAsDataURL(file);
                    } catch (error) {
                        reject(error);
                    }
                });
            }
        }
    };


    // Only add toolbar for snow theme
    if (theme === 'snow') {
        modules.toolbar = {
            container: toolbarRef,
            handlers: {
                image: function () {
                    const input = document.createElement('input');
                    input.setAttribute('type', 'file');
                    input.setAttribute('accept', 'image/*');
                    input.click();

                    input.onchange = async () => {
                        if (!input.files || !input.files[0]) return;

                        const file = input.files[0];
                        const range = this.quill.getSelection(true);

                        // Add loading placeholder
                        this.quill.insertText(range.index, 'Uploading...', {
                            'color': '#999',
                            'italic': true
                        }, true);

                        try {
                            const url = await modules.imageUploader.upload(file);
                            // Remove loading placeholder
                            this.quill.deleteText(range.index, 'Uploading...'.length);
                            // Insert the image
                            this.quill.insertEmbed(range.index, 'image', url);
                        } catch (error) {
                            // Remove loading placeholder
                            this.quill.deleteText(range.index, 'Uploading...'.length);
                            // Show error message
                            this.quill.insertText(range.index, 'Upload failed', {
                                'color': 'red',
                                'italic': true
                            }, true);
                        }
                    };
                }
            }
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
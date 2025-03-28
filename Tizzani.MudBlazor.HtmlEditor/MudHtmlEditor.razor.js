var Embed = Quill.import('blots/block/embed');
const Delta = Quill.import('delta');
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
        quill.root.addEventListener('paste', this.handlePaste);
        quill.root.addEventListener('dragenter', this.handleDragEnter);
        quill.root.addEventListener('dragover', this.handleDragOver);
        quill.root.addEventListener('drop', this.handleDrop);
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
    };

    insertDividerHandler = () => {
        const range = this.quill.getSelection();

        if (range) {
            this.quill.insertEmbed(range.index, "hr", "null");
        }
    };

    handlePaste = async (event) => {
        const clipboardData = event.clipboardData || window.clipboardData;
        const items = clipboardData.items;
        const pastedItems = Array.from(items);

        const htmlData = await new Promise((resolve) => {
            const item = Array.from(items).find(i => i.type === "text/html");
            if (item) {
                item.getAsString(resolve);
            } else {
                resolve(null);
            }
        });

        if (htmlData) {
            const updatedHtml = await this.replaceBase64Images(htmlData);
            this.quill.clipboard.dangerouslyPasteHTML(updatedHtml);
        }
    };

    replaceBase64Images = async () => {
        const editor = this.quill;
        let imgElements = editor.root.querySelectorAll("img[src^='data:image/']");

        if (imgElements.length === 0) {
            console.warn("No Base64 images found.");
            return;
        }

        for (let imgElement of imgElements) {
            let base64Src = imgElement.src;
            let base64Preview = base64Src.substring(0, 30);

            let mimeTypeMatch = base64Src.match(/data:(image\/[a-zA-Z]+);base64/);
            if (!mimeTypeMatch) {
                console.error("Failed to extract MIME type.");
                continue;
            }

            let mimeType = mimeTypeMatch[1];
            let base64String = base64Src.split(",")[1];
            let byteCharacters = atob(base64String);
            let byteNumbers = new Array(byteCharacters.length);
            let fileExtension = mimeType.split('/')[1];
            let fileName = `image_${Date.now()}.${fileExtension}`;

            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            let byteArray = new Uint8Array(byteNumbers);
            let file = new File([byteArray], fileName, { type: mimeType });

            try {
                let url = await this.uploadPastedImageAndReturnLink(file);
                if (url) {
                    console.log("Replacing:", base64Preview + "... →", url);

                    // Use Quill's root innerHTML to replace the image src
                    let html = editor.root.innerHTML;
                    let replacedHtml = html.replace(base64Src, url);

                    // Set the entire editor content with replaced HTML
                    editor.root.innerHTML = replacedHtml;
                }
            } catch (error) {
                console.error("Image replacement failed:", error);
            }
        }
    };





    handleDragEnter = (event) => {
        event.preventDefault();
        event.stopPropagation();
    };

    handleDragOver = (event) => {
        event.preventDefault();
        event.stopPropagation();
    };

    handleDrop = async (event) => {
        event.preventDefault();
        event.stopPropagation();

        const items = event.dataTransfer.items;
        const droppedItems = Array.from(items);

        for (let item of droppedItems) {
            if (item.type.indexOf('image') !== -1) {
                let file;
                if (item.kind === 'file') {
                    file = item.getAsFile();
                }

                if (file) {
                    const range = this.quill.getSelection(true);

                    // Add loading placeholder
                    this.quill.insertText(range.index, 'Uploading...', {
                        'color': '#999',
                        'italic': true
                    }, true);

                    try {
                        // Use the same upload method as image upload
                        const url = await this.quill.getModule('imageUploader').upload(file);

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
                }
            }
        }
    };
    dataURItoFile = async (dataURI) => {
        //console.log('Converting data URI to file:', dataURI); // Debug log
        const response = await fetch(dataURI);
        const blob = await response.blob();
        //console.log('Converted blob:', blob); // Debug log
        return new File([blob], 'pasted-image.png', { type: blob.type });
    };
    uploadPastedImageAndReturnLink = async (file) => {
        try {
            const url = await this.quill.getModule('imageUploader').upload(file);
            console.log("Upload successful", url); // Debug log
            return url; // Return the uploaded URL
        } catch (error) {
            console.error("Upload failed", error);
            return null; // Return null on failure
        }
    };

    uploadPastedImage = async (file) => {
        const range = this.quill.getSelection(true);

        // Add loading placeholder
        this.quill.insertText(range.index, 'Uploading...', {
            'color': '#999',
            'italic': true
        }, true);

        try {
            // Use the same upload method as image upload
            const url = await this.quill.getModule('imageUploader').upload(file);

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
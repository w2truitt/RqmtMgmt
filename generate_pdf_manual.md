# How to Generate PDF from HTML Presentation

The HTML presentation includes CSS print media rules for landscape PDF generation.

## Method 1: Browser Print to PDF (Recommended - Best Quality)

This method gives you the most control and best results:

### Firefox:
1. Open `RqmtMgmt_Presentation.html` in Firefox
2. Press `Ctrl + P` (or Cmd + P on Mac)
3. Select "Save to PDF" as destination
4. **Important Settings:**
   - **Orientation:** Landscape
   - **Margins:** None or Minimal
   - **Scale:** 100%
   - **Background graphics:** ✓ Enabled (important for colors!)
5. Click "Save"

### Chrome/Chromium:
1. Open `RqmtMgmt_Presentation.html` in Chrome
2. Press `Ctrl + P` (or Cmd + P on Mac)
3. Select "Save as PDF" as destination
4. Click "More settings"
5. **Important Settings:**
   - **Layout:** Landscape
   - **Margins:** None
   - **Scale:** Default (100%)
   - **Options:** ✓ Background graphics
   - **Pages:** All
6. Click "Save"

## Method 2: Command Line with wkhtmltopdf

If available, this works well:

```bash
wkhtmltopdf \
  --orientation Landscape \
  --page-size Letter \
  --enable-local-file-access \
  --background \
  --images \
  --javascript-delay 2000 \
  --no-stop-slow-scripts \
  RqmtMgmt_Presentation.html \
  RqmtMgmt_Presentation.pdf
```

To install wkhtmltopdf:
```bash
sudo apt install wkhtmltopdf
```

## Method 3: Automated Script (Already Provided)

A Python script has been created but Chrome's headless PDF generation
has limitations with multi-slide HTML. Use Method 1 for best results.

## Tips for Best PDF Quality

1. **Landscape orientation is crucial** - slides are designed for 16:9 aspect ratio
2. **Enable background graphics** - this includes gradient backgrounds and colors
3. **Use Firefox or Chrome** - they have the best CSS support
4. **Print from fullscreen mode (F11)** - ensures proper slide sizing
5. **One slide per page** - the CSS is configured to break pages between slides

## Expected PDF Output

- **16 pages** (one per slide)
- **Landscape orientation**
- **Full-color with gradients**
- **All screenshots embedded**
- **Terminal styling preserved**
- **File size:** ~2-5 MB (depending on screenshot compression)

## Troubleshooting

**Problem:** PDF is blank or very small
- **Solution:** Make sure background graphics are enabled in print settings

**Problem:** Images not showing
- **Solution:** Ensure the `presentation-assets/` folder is in the same directory

**Problem:** Only one page in PDF
- **Solution:** The browser might not be recognizing page breaks. Try Firefox instead.

**Problem:** Text is cut off
- **Solution:** Set margins to "None" or "Minimum"

## Quick Command (Recommended)

The easiest way:

```bash
# Open in Firefox
firefox RqmtMgmt_Presentation.html

# Then: Ctrl+P → Landscape → Save to PDF
```

Or:

```bash
# Open in Chrome
google-chrome RqmtMgmt_Presentation.html

# Then: Ctrl+P → Landscape → Background graphics → Save as PDF
```

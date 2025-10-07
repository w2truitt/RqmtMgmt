#!/usr/bin/env python3
"""
Generate a PDF presentation from the HTML file with proper landscape slides.
Uses Chrome/Chromium headless mode for high-quality output.
"""

import subprocess
import os
import sys
import json
import tempfile

# Paths
BASE_DIR = "/home/wtruitt/src/repos/RqmtMgmt"
HTML_FILE = os.path.join(BASE_DIR, "RqmtMgmt_Presentation.html")
OUTPUT_PDF = os.path.join(BASE_DIR, "RqmtMgmt_Presentation.pdf")

def find_chrome():
    """Find Chrome or Chromium executable."""
    candidates = [
        "/usr/bin/google-chrome",
        "/usr/bin/chromium",
        "/usr/bin/chromium-browser",
        "/snap/bin/chromium",
        "/snap/bin/google-chrome",
    ]
    
    for candidate in candidates:
        if os.path.exists(candidate):
            return candidate
    
    # Try which command
    try:
        result = subprocess.run(["which", "google-chrome"], 
                              capture_output=True, text=True)
        if result.returncode == 0 and result.stdout.strip():
            return result.stdout.strip()
    except:
        pass
    
    return None

def generate_pdf():
    """Generate PDF from HTML presentation."""
    
    print("🎨 Generating PDF from HTML presentation...")
    print("━" * 60)
    
    # Find Chrome
    chrome_path = find_chrome()
    if not chrome_path:
        print("❌ Chrome/Chromium not found!")
        print("   Please install: sudo apt install chromium-browser")
        return False
    
    print(f"✓ Using: {chrome_path}")
    
    # Check HTML file exists
    if not os.path.exists(HTML_FILE):
        print(f"❌ HTML file not found: {HTML_FILE}")
        return False
    
    print(f"✓ HTML file: {HTML_FILE}")
    
    # Convert to absolute file:// URL
    abs_html_path = os.path.abspath(HTML_FILE)
    file_url = f"file://{abs_html_path}"
    
    print(f"✓ URL: {file_url}")
    print("\n⏳ Generating PDF (this may take a moment)...")
    
    # Chrome DevTools Protocol settings for landscape PDF
    # Using --print-to-pdf with custom settings
    cmd = [
        chrome_path,
        "--headless",
        "--disable-gpu",
        "--disable-software-rasterizer",
        "--disable-dev-shm-usage",
        "--no-sandbox",
        f"--print-to-pdf={OUTPUT_PDF}",
        "--print-to-pdf-no-header",
        "--no-pdf-header-footer",
        "--virtual-time-budget=15000",  # Wait 15 seconds for rendering
        "--run-all-compositor-stages-before-draw",
        file_url
    ]
    
    try:
        # Run Chrome
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=30
        )
        
        # Check if PDF was created
        if os.path.exists(OUTPUT_PDF):
            size = os.path.getsize(OUTPUT_PDF)
            size_mb = size / (1024 * 1024)
            
            print("\n✅ PDF created successfully!")
            print(f"   📄 File: {OUTPUT_PDF}")
            print(f"   📦 Size: {size_mb:.2f} MB ({size:,} bytes)")
            print("\n📖 To view:")
            print(f"   $ xdg-open {OUTPUT_PDF}")
            print(f"   $ evince {OUTPUT_PDF}")
            print("\n━" * 60)
            print("✨ Your PDF is ready for distribution!")
            print("\nNote: The PDF is in portrait mode by default from Chrome.")
            print("For best landscape results, print the HTML directly to PDF")
            print("from a browser (Ctrl+P) with landscape orientation selected.")
            return True
        else:
            print(f"\n❌ PDF generation failed")
            if result.stderr:
                print(f"Error: {result.stderr}")
            return False
            
    except subprocess.TimeoutExpired:
        print("\n❌ PDF generation timed out")
        return False
    except Exception as e:
        print(f"\n❌ Error: {e}")
        return False

if __name__ == "__main__":
    success = generate_pdf()
    sys.exit(0 if success else 1)

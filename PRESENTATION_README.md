# RqmtMgmt Presentation Files

This directory contains presentation materials for the RqmtMgmt project.

## Available Presentations

### 1. PowerPoint Presentation (PPTX)
**File:** `RqmtMgmt_Presentation.pptx`
- **Format:** Microsoft PowerPoint 2007+ (.pptx)
- **Size:** 428 KB
- **Slides:** 14 slides
- **Features:**
  - Professional layout with branded colors
  - Embedded screenshots from the live application
  - Kubernetes deployment information
  - Comprehensive project overview

**How to use:**
- Open with Microsoft PowerPoint, LibreOffice Impress, or Google Slides
- Edit and customize as needed
- Export to PDF if needed for distribution

### 2. HTML Presentation
**File:** `RqmtMgmt_Presentation.html`
- **Format:** Standalone HTML file
- **Size:** 16 KB
- **Slides:** 14 slides
- **Features:**
  - Beautiful gradient design
  - Fully responsive and animated
  - Keyboard navigation (arrow keys)
  - Works in any modern browser
  - No dependencies required

**How to use:**
- Open directly in any web browser (Chrome, Firefox, Edge, Safari)
- Navigate with arrow keys or on-screen buttons
- Press F11 for fullscreen mode
- Works offline - no internet connection required

## Presentation Content

### Slide Overview
1. **Title Slide** - Project name and version
2. **Project Overview** - High-level description and goals
3. **Technical Architecture** - Technology stack and design
4. **Dashboard** - Screenshot of main dashboard
5. **Project Management** - Project organization features
6. **Requirements Management** - Hierarchical requirements system
7. **Document Management** - Specification document linking
8. **Test Case Management** - Test execution and tracking
9. **Kubernetes Deployment** - Live cluster information with pod/service details
10. **Swagger API** - RESTful API documentation
11. **Testing Excellence** - 732 comprehensive tests across all layers
12. **AI-Enhanced Development** - How AI accelerated the development process
13. **Key Features** - Core capabilities and functionality
14. **Future Roadmap** - Planned enhancements

## Screenshots Used

All screenshots are from: `/tmp/playwright-mcp-output/1759863209213/`

- `dashboard.png` - Main application dashboard
- `projects.png` - Project management interface
- `requirements.png` - Requirements listing
- `documents.png` - Document management
- `testcases-project25.png` - Test case management
- `swagger-api.png` - API documentation

## Kubernetes Information

The presentation includes live deployment information from the `rqmtmgmt` namespace:

**Active Pods:**
- backend-67cd869c47-fb4lc (Running)
- frontend-64cbfff58c-9l948 (Running)
- identityserver-7d69c95f89-cx7sb (Running)
- mssql-0 (StatefulSet - Running)

**Services:**
- backend-service (ClusterIP: 10.43.184.190)
- frontend-service (ClusterIP: 10.43.106.137)
- identityserver-service (ClusterIP: 10.43.232.135)
- mssql-service (ClusterIP: 10.43.114.158)

**Ingress:** rqmtmgmt.local (Traefik) - HTTPS enabled

## AI-Powered Development Highlights

The presentation emphasizes how AI tools accelerated development:
- GitHub Copilot for code generation
- AI-assisted architecture decisions
- Automated code review and quality analysis
- AI-generated test cases (732 total tests)
- Documentation generation
- Refactoring suggestions

## Customization

### For PowerPoint (.pptx):
1. Open in PowerPoint/Impress/Google Slides
2. Edit text, colors, and layouts as needed
3. Add or remove slides
4. Update screenshots with newer versions

### For HTML:
1. Open `RqmtMgmt_Presentation.html` in a text editor
2. Modify content within the `<div class="slide">` sections
3. Adjust colors in the `<style>` section
4. Update screenshot paths if needed

## Converting to PDF

### From PowerPoint:
- File → Save As → PDF
- Or File → Export → PDF

### From HTML:
- Open in Chrome/Edge
- Press Ctrl+P (Print)
- Select "Save as PDF" as destination
- Choose landscape orientation
- Set margins to minimum
- Print each slide individually or use browser's print-to-PDF

## Tips for Presenting

1. **HTML Presentation:**
   - Press F11 for fullscreen
   - Use arrow keys for navigation
   - Works great for web demos

2. **PowerPoint:**
   - Use Presenter View for notes
   - Test animations before presenting
   - Have PDF backup ready

## Version

- **Presentation Version:** 1.0
- **RqmtMgmt Version:** 1.0.15
- **Created:** October 2024
- **Last Updated:** October 2024

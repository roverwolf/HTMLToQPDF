# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.5.3] - 2026-07-14

### Fixed

- Images near the bottom of a page no longer shrink into leftover space. Images now use `AspectRatio(FitWidth)` so the column moves them to the next page instead of `FitArea` squeezing them into a thin strip.
- Very tall images are capped so they scale down instead of throwing `DocumentLayoutException`.
- HTML `width` / `height` attributes on `<img>` are honored as maximum display size (CSS px → pt), matching CKEditor/UI sizing so images no longer stretch to full page width.

### Changed

- Example custom style for `img` uses `MaxWidth` instead of `MaxHeight` (compatible with width-based sizing). Prefer `MaxWidth` over `MaxHeight` when constraining images.

## [1.5.2] - 2026-07-07

### Fixed

- Multiple `<img>` elements in the same parent now render correctly. Previously, only a single image per inline buffer was routed to `ImgComponent`; additional images were silently dropped by `ParagraphComponent`.

## [1.5.0] - 2025-02-06

### Added

- **New HTML Tags:**
  - `span` - Inline container for styling
  - `blockquote` - Block quotation with left border styling
  - `pre` - Preformatted text block (preserves whitespace, monospace font)
  - `code` - Inline code with monospace font and background
  - `hr` - Horizontal rule/divider
  - `mark` - Highlighted text with yellow background
  - `abbr` - Abbreviation with underline styling
  - `kbd` - Keyboard input styling
  - `samp` - Sample output (monospace)
  - `var` - Variable (italic)
  - `q` - Inline quotation (italic)

- **CSS Inline Style Parsing:**
  - New `CssParser` utility for parsing inline `style` attributes
  - Text properties: `color`, `background-color`, `font-size`, `font-weight`, `font-style`, `font-family`, `text-decoration`, `letter-spacing`, `line-height`
  - Container properties: `padding` (all directions), `margin`, `background-color`, `border`, `border-color`, `width`, `height`, `min-*`, `max-*`, `text-align`
  - Color formats: hex (`#RGB`, `#RRGGBB`, `#RRGGBBAA`), `rgb()`, `rgba()`, named colors
  - Length units: `px`, `pt`, `em`, `rem`, `cm`, `mm`, `in`, `%`

### Changed

- Updated `ParagraphComponent` to apply inline CSS text styles
- Updated `BaseHTMLComponent` to apply inline CSS container styles
- Improved documentation in README with comprehensive feature list

## [1.4.5] - 2025-01-XX

### Fixed

- Fixed non-breaking spaces (`&nbsp;`) being removed in nested inline elements

## [1.4.4] - 2024-XX-XX

### Changed

- Updated QuestPDF to 2025.12.1

## [1.4.3] - 2024-XX-XX

### Changed

- Updated QuestPDF to 2025.7.4

## [1.4.2] - 2024-XX-XX

### Changed

- Updated QuestPDF version
- Updated NuGet package version

## [1.4.1] - 2024-XX-XX

### Fixed

- Image alignment and sizing improvements (AlignCenter, FitArea)

## [1.4.0] - 2024-XX-XX

### Changed

- Updated ImgUtils to use HttpClient instead of deprecated WebClient
- Added base64 image support to default image handler

## [1.3.0] - 2024-XX-XX

### Fixed

- Unit conversion fix (Mill to Mil)

## Earlier Versions

For changes prior to version 1.3.0, please refer to the [commit history](https://github.com/JeremyVm/HTMLToQPDF/commits/master).

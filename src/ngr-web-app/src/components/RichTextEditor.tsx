import { useEditor, EditorContent } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Underline from '@tiptap/extension-underline';
import TiptapLink from '@tiptap/extension-link';
import Image from '@tiptap/extension-image';
import Highlight from '@tiptap/extension-highlight';
import { TextStyle } from '@tiptap/extension-text-style';
import { Color } from '@tiptap/extension-color';
import Placeholder from '@tiptap/extension-placeholder';
import {
  Box,
  Divider,
  IconButton,
  ToggleButton,
  ToggleButtonGroup,
  Tooltip,
  Select,
  MenuItem,
  type SelectChangeEvent,
} from '@mui/material';
import FormatBoldIcon from '@mui/icons-material/FormatBold';
import FormatItalicIcon from '@mui/icons-material/FormatItalic';
import FormatUnderlinedIcon from '@mui/icons-material/FormatUnderlined';
import StrikethroughSIcon from '@mui/icons-material/StrikethroughS';
import FormatListBulletedIcon from '@mui/icons-material/FormatListBulleted';
import FormatListNumberedIcon from '@mui/icons-material/FormatListNumbered';
import CodeIcon from '@mui/icons-material/Code';
import FormatQuoteIcon from '@mui/icons-material/FormatQuote';
import LinkIcon from '@mui/icons-material/Link';
import ImageIcon from '@mui/icons-material/Image';

// ── Predefined brand colors ──────────────────────────────────────

const BRAND_COLORS = [
  { label: 'Navy', value: '#003B71' },
  { label: 'Blue', value: '#005DA8' },
  { label: 'Gold', value: '#F5A623' },
  { label: 'Dark', value: '#1A1A2E' },
  { label: 'Gray', value: '#4A5568' },
  { label: 'Red', value: '#D32F2F' },
  { label: 'Green', value: '#2E7D32' },
  { label: 'Black', value: '#000000' },
];

// ── Props ────────────────────────────────────────────────────────

interface RichTextEditorProps {
  content: string;
  onChange: (html: string) => void;
  placeholder?: string;
  maxLength?: number;
  minHeight?: number;
}

export function RichTextEditor({
  content,
  onChange,
  placeholder = 'Start typing...',
  maxLength = 5000,
  minHeight = 200,
}: RichTextEditorProps) {
  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        heading: { levels: [2, 3, 4] },
      }),
      Underline,
      TiptapLink.configure({ openOnClick: false }),
      Image,
      Highlight,
      TextStyle,
      Color,
      Placeholder.configure({ placeholder }),
    ],
    content,
    onUpdate: ({ editor: ed }) => {
      const html = ed.getHTML();
      // Always propagate changes — parent can enforce limits if needed.
      // Character count display warns users visually when approaching the limit.
      onChange(html);
    },
  });

  if (!editor) return null;

  const handleHeading = (e: SelectChangeEvent<string>) => {
    const val = e.target.value;
    if (val === 'paragraph') {
      editor.chain().focus().setParagraph().run();
    } else {
      const level = Number(val) as 2 | 3 | 4;
      editor.chain().focus().toggleHeading({ level }).run();
    }
  };

  const handleColor = (e: SelectChangeEvent<string>) => {
    editor.chain().focus().setColor(e.target.value).run();
  };

  const handleLink = () => {
    const url = window.prompt('Enter URL:');
    if (url) {
      editor.chain().focus().setLink({ href: url }).run();
    }
  };

  const handleImage = () => {
    const url = window.prompt('Enter image URL:');
    if (url) {
      editor.chain().focus().setImage({ src: url }).run();
    }
  };

  const currentHeading = editor.isActive('heading', { level: 2 })
    ? '2'
    : editor.isActive('heading', { level: 3 })
      ? '3'
      : editor.isActive('heading', { level: 4 })
        ? '4'
        : 'paragraph';

  const charCount = editor.getText().length;

  return (
    <Box
      sx={{
        border: '1px solid',
        borderColor: 'divider',
        borderRadius: 1,
        overflow: 'hidden',
      }}
    >
      {/* ── Toolbar ──────────────────────────────────────────── */}
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
          gap: 0.5,
          p: 1,
          bgcolor: 'background.paper',
          borderBottom: '1px solid',
          borderColor: 'divider',
        }}
        role="toolbar"
        aria-label="Text formatting"
      >
        {/* Heading select */}
        <Select
          size="small"
          value={currentHeading}
          onChange={handleHeading}
          sx={{ minWidth: 120, height: 32 }}
          aria-label="Text style"
        >
          <MenuItem value="paragraph">Paragraph</MenuItem>
          <MenuItem value="2">Heading 2</MenuItem>
          <MenuItem value="3">Heading 3</MenuItem>
          <MenuItem value="4">Heading 4</MenuItem>
        </Select>

        <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />

        {/* Formatting buttons */}
        <ToggleButtonGroup size="small" aria-label="Text formatting">
          <ToggleButton
            value="bold"
            selected={editor.isActive('bold')}
            onClick={() => editor.chain().focus().toggleBold().run()}
            aria-label="Bold"
          >
            <FormatBoldIcon fontSize="small" />
          </ToggleButton>
          <ToggleButton
            value="italic"
            selected={editor.isActive('italic')}
            onClick={() => editor.chain().focus().toggleItalic().run()}
            aria-label="Italic"
          >
            <FormatItalicIcon fontSize="small" />
          </ToggleButton>
          <ToggleButton
            value="underline"
            selected={editor.isActive('underline')}
            onClick={() => editor.chain().focus().toggleUnderline().run()}
            aria-label="Underline"
          >
            <FormatUnderlinedIcon fontSize="small" />
          </ToggleButton>
          <ToggleButton
            value="strike"
            selected={editor.isActive('strike')}
            onClick={() => editor.chain().focus().toggleStrike().run()}
            aria-label="Strikethrough"
          >
            <StrikethroughSIcon fontSize="small" />
          </ToggleButton>
        </ToggleButtonGroup>

        <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />

        {/* Color select */}
        <Select
          size="small"
          value=""
          onChange={handleColor}
          displayEmpty
          sx={{ minWidth: 80, height: 32 }}
          aria-label="Text color"
          renderValue={() => 'Color'}
        >
          {BRAND_COLORS.map((c) => (
            <MenuItem key={c.value} value={c.value}>
              <Box
                component="span"
                sx={{
                  display: 'inline-block',
                  width: 14,
                  height: 14,
                  bgcolor: c.value,
                  borderRadius: '50%',
                  mr: 1,
                  verticalAlign: 'middle',
                  border: '1px solid #ccc',
                }}
              />
              {c.label}
            </MenuItem>
          ))}
        </Select>

        <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />

        {/* Lists */}
        <Tooltip title="Bulleted list">
          <IconButton
            size="small"
            onClick={() => editor.chain().focus().toggleBulletList().run()}
            color={editor.isActive('bulletList') ? 'primary' : 'default'}
            aria-label="Bulleted list"
          >
            <FormatListBulletedIcon fontSize="small" />
          </IconButton>
        </Tooltip>
        <Tooltip title="Numbered list">
          <IconButton
            size="small"
            onClick={() => editor.chain().focus().toggleOrderedList().run()}
            color={editor.isActive('orderedList') ? 'primary' : 'default'}
            aria-label="Numbered list"
          >
            <FormatListNumberedIcon fontSize="small" />
          </IconButton>
        </Tooltip>

        <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />

        {/* Code block */}
        <Tooltip title="Code block">
          <IconButton
            size="small"
            onClick={() => editor.chain().focus().toggleCodeBlock().run()}
            color={editor.isActive('codeBlock') ? 'primary' : 'default'}
            aria-label="Code block"
          >
            <CodeIcon fontSize="small" />
          </IconButton>
        </Tooltip>

        {/* Blockquote */}
        <Tooltip title="Blockquote">
          <IconButton
            size="small"
            onClick={() => editor.chain().focus().toggleBlockquote().run()}
            color={editor.isActive('blockquote') ? 'primary' : 'default'}
            aria-label="Blockquote"
          >
            <FormatQuoteIcon fontSize="small" />
          </IconButton>
        </Tooltip>

        {/* Link */}
        <Tooltip title="Insert link">
          <IconButton
            size="small"
            onClick={handleLink}
            color={editor.isActive('link') ? 'primary' : 'default'}
            aria-label="Insert link"
          >
            <LinkIcon fontSize="small" />
          </IconButton>
        </Tooltip>

        {/* Image */}
        <Tooltip title="Insert image">
          <IconButton size="small" onClick={handleImage} aria-label="Insert image">
            <ImageIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </Box>

      {/* ── Editor content ───────────────────────────────────── */}
      <Box
        sx={{
          minHeight,
          p: 2,
          '& .ProseMirror': {
            outline: 'none',
            minHeight: minHeight - 32,
            '& p.is-editor-empty:first-child::before': {
              color: 'text.disabled',
              content: 'attr(data-placeholder)',
              float: 'left',
              height: 0,
              pointerEvents: 'none',
            },
          },
          '& .ProseMirror h2': { fontSize: '1.5rem', fontWeight: 700, mt: 2, mb: 1 },
          '& .ProseMirror h3': { fontSize: '1.25rem', fontWeight: 700, mt: 2, mb: 1 },
          '& .ProseMirror h4': { fontSize: '1.1rem', fontWeight: 600, mt: 2, mb: 1 },
          '& .ProseMirror blockquote': {
            borderLeft: '3px solid #E2E8F0',
            pl: 2,
            ml: 0,
            color: 'text.secondary',
          },
          '& .ProseMirror pre': {
            bgcolor: '#f5f5f5',
            p: 2,
            borderRadius: 1,
            fontFamily: 'monospace',
            overflow: 'auto',
          },
          '& .ProseMirror img': { maxWidth: '100%', height: 'auto' },
          '& .ProseMirror a': { color: '#005DA8' },
        }}
      >
        <EditorContent editor={editor} />
      </Box>

      {/* ── Character count ──────────────────────────────────── */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flex-end',
          px: 2,
          py: 0.5,
          bgcolor: 'background.paper',
          borderTop: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Box
          component="span"
          sx={{
            fontSize: '0.75rem',
            color: charCount > maxLength * 0.9 ? 'error.main' : 'text.secondary',
          }}
        >
          {charCount.toLocaleString()} / {maxLength.toLocaleString()}
        </Box>
      </Box>
    </Box>
  );
}

import { useState, useCallback, useMemo } from 'react';
import {
  Box,
  CircularProgress,
  FormControl,
  InputAdornment,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  type SelectChangeEvent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TableSortLabel,
  TextField,
  Typography,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';

// ── Column definition ────────────────────────────────────────────

export interface DataTableColumn<T> {
  /** Unique key matching a field on the row object */
  id: string;
  /** Display label in header */
  label: string;
  /** Data type for sort comparison (default: 'text') */
  dataType?: 'text' | 'number' | 'date';
  /** Custom render function */
  render?: (row: T) => React.ReactNode;
  /** Whether the column is sortable (default: true) */
  sortable?: boolean;
  /** Column alignment (default: 'left') */
  align?: 'left' | 'center' | 'right';
  /** Min width */
  minWidth?: number;
}

// ── Props ────────────────────────────────────────────────────────

export interface DataTableProps<T> {
  /** Column definitions */
  columns: DataTableColumn<T>[];
  /** Row data */
  rows: T[];
  /** Unique key extractor */
  getRowId: (row: T) => string | number;
  /** Total row count from server (for server-side pagination). If -1, unknown. */
  totalCount?: number;
  /** Whether data is loading */
  loading?: boolean;
  /** Empty state message */
  emptyMessage?: string;
  /** Show search bar (default: true) */
  searchable?: boolean;
  /** Search placeholder */
  searchPlaceholder?: string;
  /** Row click handler */
  onRowClick?: (row: T) => void;
  /** Render actions column */
  renderActions?: (row: T) => React.ReactNode;

  // ── Server-side mode props ──────────────────────────────────────
  /** If true, pagination/search/sort are handled server-side. Parent must supply callbacks. */
  serverSide?: boolean;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  onSearchChange?: (search: string) => void;
  onSortChange?: (sortBy: string, sortDirection: 'asc' | 'desc') => void;
  /** Current page (0-indexed, for controlled mode) */
  page?: number;
  /** Current page size (for controlled mode) */
  pageSize?: number;
  /** Current search term (for controlled mode) */
  search?: string;
  /** Current sort column (for controlled mode) */
  sortBy?: string;
  /** Current sort direction (for controlled mode) */
  sortDirection?: 'asc' | 'desc';
}

// ── Page size options ────────────────────────────────────────────

const PAGE_SIZE_OPTIONS = [25, 50, 100];

// ── Component ────────────────────────────────────────────────────

export function DataTable<T>({
  columns,
  rows,
  getRowId,
  totalCount,
  loading = false,
  emptyMessage = 'No records found',
  searchable = true,
  searchPlaceholder = 'Search...',
  onRowClick,
  renderActions,
  serverSide = false,
  onPageChange,
  onPageSizeChange,
  onSearchChange,
  onSortChange,
  page: controlledPage,
  pageSize: controlledPageSize,
  search: controlledSearch,
  sortBy: controlledSortBy,
  sortDirection: controlledSortDirection,
}: DataTableProps<T>) {
  // ── Local state (client-side mode) ──────────────────────────────
  const [localPage, setLocalPage] = useState(0);
  const [localPageSize, setLocalPageSize] = useState(25);
  const [localSearch, setLocalSearch] = useState('');
  const [localSortBy, setLocalSortBy] = useState('');
  const [localSortDirection, setLocalSortDirection] = useState<'asc' | 'desc'>('asc');

  // ── Resolve controlled vs uncontrolled ──────────────────────────
  const page = serverSide ? (controlledPage ?? 0) : localPage;
  const pageSize = serverSide ? (controlledPageSize ?? 25) : localPageSize;
  const search = serverSide ? (controlledSearch ?? '') : localSearch;
  const sortBy = serverSide ? (controlledSortBy ?? '') : localSortBy;
  const sortDirection = serverSide ? (controlledSortDirection ?? 'asc') : localSortDirection;

  // ── Client-side filtering, sorting, pagination ──────────────────
  const processedRows = useMemo(() => {
    if (serverSide) return rows;

    let result = [...rows];

    // Search filter
    if (search) {
      const term = search.toLowerCase();
      result = result.filter((row) =>
        columns.some((col) => {
          const val = (row as Record<string, unknown>)[col.id];
          return val != null && String(val).toLowerCase().includes(term);
        }),
      );
    }

    // Sort
    if (sortBy) {
      const col = columns.find((c) => c.id === sortBy);
      const dataType = col?.dataType ?? 'text';
      result.sort((a, b) => {
        const aVal = (a as Record<string, unknown>)[sortBy];
        const bVal = (b as Record<string, unknown>)[sortBy];
        let cmp = 0;
        if (aVal == null && bVal == null) cmp = 0;
        else if (aVal == null) cmp = -1;
        else if (bVal == null) cmp = 1;
        else if (dataType === 'number') cmp = Number(aVal) - Number(bVal);
        else if (dataType === 'date') cmp = new Date(String(aVal)).getTime() - new Date(String(bVal)).getTime();
        else cmp = String(aVal).localeCompare(String(bVal));
        return sortDirection === 'desc' ? -cmp : cmp;
      });
    }

    return result;
  }, [rows, search, sortBy, sortDirection, columns, serverSide]);

  const paginatedRows = useMemo(() => {
    if (serverSide) return rows;
    const start = page * pageSize;
    return processedRows.slice(start, start + pageSize);
  }, [processedRows, page, pageSize, serverSide, rows]);

  const effectiveTotalCount = serverSide
    ? (totalCount ?? -1)
    : processedRows.length;

  // ── Handlers ────────────────────────────────────────────────────
  const handlePageChange = useCallback(
    (_: unknown, newPage: number) => {
      if (serverSide) onPageChange?.(newPage);
      else setLocalPage(newPage);
    },
    [serverSide, onPageChange],
  );

  const handlePageSizeChange = useCallback(
    (e: SelectChangeEvent<number>) => {
      const newSize = Number(e.target.value);
      if (serverSide) {
        onPageSizeChange?.(newSize);
        onPageChange?.(0);
      } else {
        setLocalPageSize(newSize);
        setLocalPage(0);
      }
    },
    [serverSide, onPageSizeChange, onPageChange],
  );

  const handleSearchChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const val = e.target.value;
      if (serverSide) {
        onSearchChange?.(val);
        onPageChange?.(0);
      } else {
        setLocalSearch(val);
        setLocalPage(0);
      }
    },
    [serverSide, onSearchChange, onPageChange],
  );

  const handleSort = useCallback(
    (columnId: string) => {
      const isAsc = sortBy === columnId && sortDirection === 'asc';
      const newDir = isAsc ? 'desc' : 'asc';
      if (serverSide) onSortChange?.(columnId, newDir);
      else {
        setLocalSortBy(columnId);
        setLocalSortDirection(newDir);
      }
    },
    [sortBy, sortDirection, serverSide, onSortChange],
  );

  const allColumns = useMemo(() => {
    if (!renderActions) return columns;
    return [
      ...columns,
      {
        id: '__actions',
        label: 'Actions',
        sortable: false,
        align: 'right' as const,
      } as DataTableColumn<T>,
    ];
  }, [columns, renderActions]);

  return (
    <Box>
      {/* ── Search + page size controls ──────────────────────────── */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2, gap: 2 }}>
        {searchable ? (
          <TextField
            size="small"
            placeholder={searchPlaceholder}
            value={search}
            onChange={handleSearchChange}
            sx={{ width: 320 }}
            aria-label="Search table"
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" color="action" />
                </InputAdornment>
              ),
            }}
          />
        ) : (
          <Box />
        )}
        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel id="page-size-label">Rows</InputLabel>
          <Select
            labelId="page-size-label"
            value={pageSize as unknown as ''}
            label="Rows"
            onChange={handlePageSizeChange as (e: SelectChangeEvent<''>) => void}
          >
            {PAGE_SIZE_OPTIONS.map((opt) => (
              <MenuItem key={opt} value={opt}>
                {opt}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      {/* ── Table ────────────────────────────────────────────────── */}
      <TableContainer component={Paper}>
        <Table size="small" aria-label="data table">
          <TableHead>
            <TableRow>
              {allColumns.map((col) => (
                <TableCell
                  key={col.id}
                  align={col.align}
                  sx={{ minWidth: col.minWidth }}
                >
                  {col.sortable !== false ? (
                    <TableSortLabel
                      active={sortBy === col.id}
                      direction={sortBy === col.id ? sortDirection : 'asc'}
                      onClick={() => handleSort(col.id)}
                      sx={{
                        color: 'inherit !important',
                        '&.Mui-active': { color: 'inherit !important' },
                        '& .MuiTableSortLabel-icon': { color: 'rgba(255,255,255,0.7) !important' },
                      }}
                    >
                      {col.label}
                    </TableSortLabel>
                  ) : (
                    col.label
                  )}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={allColumns.length} align="center" sx={{ py: 6 }}>
                  <CircularProgress size={28} />
                </TableCell>
              </TableRow>
            ) : paginatedRows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={allColumns.length} align="center" sx={{ py: 6 }}>
                  <Typography color="text.secondary">{emptyMessage}</Typography>
                </TableCell>
              </TableRow>
            ) : (
              paginatedRows.map((row) => (
                <TableRow
                  key={getRowId(row)}
                  hover
                  sx={onRowClick ? { cursor: 'pointer' } : undefined}
                  onClick={() => onRowClick?.(row)}
                  tabIndex={0}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && onRowClick) onRowClick(row);
                  }}
                >
                  {columns.map((col) => (
                    <TableCell key={col.id} align={col.align}>
                      {col.render
                        ? col.render(row)
                        : String((row as Record<string, unknown>)[col.id] ?? '—')}
                    </TableCell>
                  ))}
                  {renderActions && (
                    <TableCell align="right">{renderActions(row)}</TableCell>
                  )}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* ── Pagination ───────────────────────────────────────────── */}
      <TablePagination
        component="div"
        count={effectiveTotalCount}
        page={page}
        rowsPerPage={pageSize}
        rowsPerPageOptions={PAGE_SIZE_OPTIONS}
        onPageChange={handlePageChange}
        onRowsPerPageChange={(e) => {
          const size = parseInt(e.target.value, 10);
          if (serverSide) {
            onPageSizeChange?.(size);
            onPageChange?.(0);
          } else {
            setLocalPageSize(size);
            setLocalPage(0);
          }
        }}
        labelDisplayedRows={({ from, to, count }) =>
          count !== -1 ? `${from}–${to} of ${count}` : `${from}–${to}`
        }
      />
    </Box>
  );
}

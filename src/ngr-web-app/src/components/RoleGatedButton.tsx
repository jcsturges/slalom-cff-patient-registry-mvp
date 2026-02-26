import { Button, Tooltip } from '@mui/material';
import type { ButtonProps } from '@mui/material';

interface RoleGatedButtonProps extends ButtonProps {
  /** When false the button is disabled and a tooltip explains why */
  allowed: boolean;
  /** Message shown in the tooltip when not allowed */
  disabledReason?: string;
}

/**
 * A Button that is disabled (never hidden) when the user lacks permission,
 * with a tooltip explaining why. Wraps the disabled button in a <span> so
 * MUI Tooltip can attach to it (disabled elements don't fire mouse events).
 */
export function RoleGatedButton({
  allowed,
  disabledReason = "You don't have permission to perform this action.",
  children,
  ...props
}: RoleGatedButtonProps) {
  if (allowed) {
    return <Button {...props}>{children}</Button>;
  }

  return (
    <Tooltip title={disabledReason} arrow placement="top">
      {/* span required: disabled buttons suppress pointer events needed by Tooltip */}
      <span style={{ display: 'inline-flex' }}>
        <Button {...props} disabled>
          {children}
        </Button>
      </span>
    </Tooltip>
  );
}

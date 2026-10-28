import { type ReactNode } from 'react';
import {
  AppBar,
  Avatar,
  Box,
  Container,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  Toolbar,
  Tooltip,
  Typography
} from '@mui/material';
import InboxIcon from '@mui/icons-material/Inbox';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import LightModeIcon from '@mui/icons-material/LightMode';
import DarkModeIcon from '@mui/icons-material/DarkMode';
import LogoutIcon from '@mui/icons-material/Logout';
import { Link, useLocation } from 'react-router-dom';
import { useMsal } from '@azure/msal-react';
import { useColorMode } from '@/theme/ThemeContext';
import { useMe } from '@/hooks/useMe';

const drawerWidth = 240;

export default function Layout({ children }: { children: ReactNode }) {
  const { instance } = useMsal();
  const { mode, toggle } = useColorMode();
  const { data: me } = useMe();
  const loc = useLocation();

  const items = [
    { to: '/solicitudes', label: 'Solicitudes', icon: <InboxIcon /> },
    { to: '/solicitudes/nueva', label: 'Nueva solicitud', icon: <AddCircleOutlineIcon /> }
  ];

  return (
    <Box sx={{ display: 'flex', minHeight: '100dvh' }}>
      <AppBar position="fixed" sx={{ zIndex: (t) => t.zIndex.drawer + 1 }}>
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            Solicitudes
          </Typography>
          <Tooltip title={mode === 'light' ? 'Modo oscuro' : 'Modo claro'}>
            <IconButton color="inherit" onClick={toggle}>
              {mode === 'light' ? <DarkModeIcon /> : <LightModeIcon />}
            </IconButton>
          </Tooltip>
          {me && (
            <Stack direction="row" spacing={1} alignItems="center" sx={{ ml: 2 }}>
              <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.dark' }}>
                {me.nombreCompleto.charAt(0).toUpperCase()}
              </Avatar>
              <Box sx={{ display: { xs: 'none', sm: 'block' }, lineHeight: 1.1 }}>
                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                  {me.nombreCompleto}
                </Typography>
                <Typography variant="caption" sx={{ opacity: 0.85 }}>
                  {me.roles.join(' · ')}
                </Typography>
              </Box>
              <Tooltip title="Cerrar sesión">
                <IconButton color="inherit" onClick={() => void instance.logoutRedirect()}>
                  <LogoutIcon />
                </IconButton>
              </Tooltip>
            </Stack>
          )}
        </Toolbar>
      </AppBar>

      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          [`& .MuiDrawer-paper`]: { width: drawerWidth, boxSizing: 'border-box' }
        }}
      >
        <Toolbar />
        <Box sx={{ overflow: 'auto', mt: 1 }}>
          <List>
            {items.map((it) => (
              <ListItemButton
                key={it.to}
                component={Link}
                to={it.to}
                selected={loc.pathname === it.to}
              >
                <ListItemIcon>{it.icon}</ListItemIcon>
                <ListItemText primary={it.label} />
              </ListItemButton>
            ))}
          </List>
        </Box>
      </Drawer>

      <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
        <Toolbar />
        <Container maxWidth="xl" disableGutters>
          {children}
        </Container>
      </Box>
    </Box>
  );
}

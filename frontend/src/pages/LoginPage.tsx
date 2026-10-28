import { Box, Button, Card, CardContent, Stack, Typography } from '@mui/material';
import LoginIcon from '@mui/icons-material/Login';
import { useMsal } from '@azure/msal-react';
import { loginRequest } from '@/auth/authConfig';

export default function LoginPage() {
  const { instance } = useMsal();

  const handleLogin = () => {
    void instance.loginRedirect(loginRequest);
  };

  return (
    <Box
      sx={{
        minHeight: '100dvh',
        display: 'grid',
        placeItems: 'center',
        background: (t) =>
          `linear-gradient(135deg, ${t.palette.primary.dark}, ${t.palette.primary.main})`,
        p: 2
      }}
    >
      <Card sx={{ maxWidth: 420, width: '100%', borderRadius: 3 }}>
        <CardContent sx={{ p: 4 }}>
          <Stack spacing={3} alignItems="center" textAlign="center">
            <Typography variant="h4" sx={{ color: 'primary.main', fontWeight: 700 }}>
              Solicitudes
            </Typography>
            <Typography color="text.secondary">
              Inicia sesión con tu cuenta corporativa de Microsoft Entra ID.
            </Typography>
            <Button
              fullWidth
              variant="contained"
              size="large"
              startIcon={<LoginIcon />}
              onClick={handleLogin}
            >
              Iniciar sesión con Microsoft
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}

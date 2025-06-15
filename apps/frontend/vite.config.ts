import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const backendUrl = env.VITE_API_BASE_URL || 'http://localhost:7071/api';
  const baseBackendUrl = backendUrl.replace('/api', '');
  
  console.log(`🔧 Vite config for ${mode} mode:`);
  console.log(`   Backend URL: ${baseBackendUrl}`);
  console.log(`   API Base: ${backendUrl}`);

  return {
    plugins: [react()],
    
    optimizeDeps: {
      exclude: ["lucide-react"],
    },
    
    build: {
      commonjsOptions: {
        include: [/lucide-react/, /node_modules/],
      },
      outDir: "dist",
      sourcemap: mode === 'development',
    },

    define: {
      __APP_VERSION__: JSON.stringify(env.VITE_APP_VERSION || '1.0.0'),
      __BUILD_DATE__: JSON.stringify(new Date().toISOString()),
    },

    server: {
      port: 3000,
      host: true,
      proxy: {
        '/api': {
          target: baseBackendUrl,
          changeOrigin: true,
          secure: false,
          configure: (proxy, _options) => {
            proxy.on('error', (err, _req, _res) => {
              console.log('🚨 Proxy error:', err);
            });
            proxy.on('proxyReq', (proxyReq, req, _res) => {
              console.log(`🔄 Proxying: ${req.method} ${req.url} -> ${baseBackendUrl}${req.url}`);
            });
            proxy.on('proxyRes', (proxyRes, req, _res) => {
              console.log(`✅ Proxy response: ${req.url} -> ${proxyRes.statusCode}`);
            });
          },
        },
      },

      cors: {
        origin: true,
        credentials: true,
      },
    },

    envPrefix: 'VITE_',

    preview: {
      port: 3000,
      host: true,
      proxy: {
        '/api': {
          target: baseBackendUrl,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});
const toastStyles = {
  success: {
    container: 'border-emerald-200 bg-emerald-50 text-emerald-950',
    accent: 'bg-emerald-500',
    label: 'Success'
  },
  error: {
    container: 'border-red-200 bg-red-50 text-red-950',
    accent: 'bg-red-500',
    label: 'Error'
  },
  info: {
    container: 'border-sky-200 bg-sky-50 text-sky-950',
    accent: 'bg-sky-500',
    label: 'Notice'
  }
};

/* eslint-disable react/prop-types */
function SuccessToast({ toasts }) {
  if (!toasts.length) return null;

  return (
    <div className="pointer-events-none fixed right-5 top-5 z-50 flex w-full max-w-sm flex-col gap-3">
      {toasts.map((toast) => {
        const style = toastStyles[toast.type] || toastStyles.info;

        return (
          <div
            key={toast.id}
            className={`pointer-events-auto overflow-hidden rounded-2xl border shadow-2xl backdrop-blur toast-slide-in ${style.container}`}
          >
            <div className="flex items-start gap-3 p-4">
              <div className={`mt-1 h-2.5 w-2.5 shrink-0 rounded-full ${style.accent}`} />
              <div className="min-w-0">
                <p className="text-xs font-bold uppercase tracking-[0.18em] opacity-70">
                  {style.label}
                </p>
                <p className="mt-1 text-sm font-semibold">{toast.message}</p>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}

export default SuccessToast;

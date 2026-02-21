const Modal = ({ title, onClose, children, footer }) => (
  <div className="modal-backdrop" role="dialog" aria-modal="true">
    <div className="modal-card">
      <header>
        <h2>{title}</h2>
        <button type="button" className="ghost close" onClick={onClose}>
          Close
        </button>
      </header>
      <div className="modal-body">{children}</div>
      {footer && <div className="modal-footer">{footer}</div>}
    </div>
  </div>
);

export default Modal;

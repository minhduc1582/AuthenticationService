const PageLoader = ({ label = 'Loading...' }) => (
  <div className="page-loader">
    <div className="spinner" aria-hidden="true" />
    <p>{label}</p>
  </div>
);

export default PageLoader;
